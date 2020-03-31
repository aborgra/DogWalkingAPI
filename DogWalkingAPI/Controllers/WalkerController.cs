using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DogWalkingAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DogWalkingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalkerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public WalkerController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int? neighborhoodId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT w.Id, w.Name, w.NeighborhoodId, ";
                if(neighborhoodId != null)
                {
                    cmd.CommandText += "n.id as 'Neighborhood Id', n.Name as 'Neighborhood Name'";
                }
                
                cmd.CommandText += "FROM Walker w LEFT JOIN Neighborhood n on w.NeighborhoodId = n.Id";

                if (neighborhoodId != null)
                {
                    cmd.CommandText += " WHERE neighborhoodId = @neighborhoodId";
                    cmd.Parameters.Add(new SqlParameter("@neighborhoodId", neighborhoodId));
                }
                SqlDataReader reader = cmd.ExecuteReader();

                List<Walker> walkers = new List<Walker>();

                while (reader.Read())
                {

                    Walker walker = new Walker
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                        Neighborhood = new Neighborhood
                        {
                            Name = reader.GetString(reader.GetOrdinal("Neighborhood Name")),
                            Id = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"))
                        }
                    };

                    walkers.Add(walker);
                }
                reader.Close();
                return Ok(walkers);
            }
        }

        [HttpGet("{id}", Name = "GetWalker")]
        public async Task<IActionResult> Get([FromRoute] int id,
            [FromQuery]string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT w.Id, w.Name, w.NeighborhoodId, n.Name as 'Neighborhood Name' ";

                    if (include == "walks")
                    {
                        cmd.CommandText += ", wks.Id as WalksId, wks.Date, wks.Duration, wks.WalkerId as WalkerId, wks.DogId as DogId";
                    }

                   cmd.CommandText += " FROM Walker w ";

                    if(include == "walks")
                    {
                        cmd.CommandText += " LEFT JOIN Walks wks on w.Id = wks.WalkerId ";
                    }

                    cmd.CommandText += "LEFT JOIN Neighborhood n on w.NeighborhoodId = n.Id WHERE w.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Walker walker = null;

                    while (reader.Read())
                    {
                        if (walker == null)
                        {
                            walker = new Walker
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("neighborhoodId")),
                                Neighborhood = new Neighborhood
                                {
                                    Name = reader.GetString(reader.GetOrdinal("Neighborhood Name"))
                                },
                                Walks = new List<Walk>()
                             };
                        }
                        if(include == "walks")
                        {
                            walker.Walks.Add(new Walk()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("WalksId")),
                                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                Duration = reader.GetInt32(reader.GetOrdinal("Duration")),
                                WalkerId = reader.GetInt32(reader.GetOrdinal("WalkerId")),
                                DogId = reader.GetInt32(reader.GetOrdinal("DogId"))
                            });
                        }
                       
                    }

                    reader.Close();
                    return Ok(walker);
                }
            }
        }



        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Walker walker)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Walker (Name, NeighborhoodId) OUTPUT INSERTED.Id Values (@Name, @NeighborhoodId)";
                    cmd.Parameters.Add(new SqlParameter("@Name", walker.Name));
                    cmd.Parameters.Add(new SqlParameter("@NeighborhoodId", walker.NeighborhoodId));

                    int newId = (int)cmd.ExecuteScalar();
                    walker.Id = newId;
                    return CreatedAtRoute("GetWalker", new { id = newId }, walker);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Walker walker)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Walker
                                     SET NeighborhoodId = @neighborhoodId
                                     WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@neighborhoodId", walker.NeighborhoodId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!WalkerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Walker WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!WalkerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool WalkerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, Address, NeighborhoodId, Neighborhood, Walks FROM Walker ";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}