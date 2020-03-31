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
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT w.Id, w.Name, w.NeighborhoodId, n.id as 'Neighborhood Id', n.Name as 'Neighborhood Name' FROM Walker w LEFT JOIN Neighborhood n on w.NeighborhoodId = n.Id";
                SqlDataReader reader = cmd.ExecuteReader();

                List<Walker> walkers = new List<Walker>();

                while (reader.Read())
                {

                    int idColumnPosition = reader.GetOrdinal("Id");
                    int idValue = reader.GetInt32(idColumnPosition);

                    int nameColumnPosition = reader.GetOrdinal("Name");
                    string nameValue = reader.GetString(nameColumnPosition);

                    int neighborhoodColumnPosition = reader.GetOrdinal("NeighborhoodId");
                    int neighborhoodValue = reader.GetInt32(neighborhoodColumnPosition);

                    int neighborhoodNameColumnPosition = reader.GetOrdinal("Neighborhood Name");
                    string neighborhoodNameValue = reader.GetString(neighborhoodNameColumnPosition);
                    int nIdColumnPosition = reader.GetOrdinal("Neighborhood Id");
                    int nIdValue = reader.GetInt32(nIdColumnPosition);

                    Walker walker = new Walker
                    {
                        Id = idValue,
                        Name = nameValue,
                        NeighborhoodId = neighborhoodValue,
                        Neighborhood = new Neighborhood
                        {
                            Name = neighborhoodNameValue,
                            Id = nIdColumnPosition 
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
                        cmd.CommandText += ", wks.Id as WalksId, wks.Date, wks.Duration, wks.WalkerId, wks.DogId";
                    }

                   cmd.CommandText += " FROM Walker w ";

                    if(include == "walks")
                    {
                        cmd.CommandText += " AND LEFT JOIN Walks wks on wks.WalkerId = w.Id";
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
                                DogId = reader.GetInt32(reader.GetOrdinal("WalkerId"))
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