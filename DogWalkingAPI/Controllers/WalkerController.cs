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
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT w.Id, w.Name, w.NeighborhoodId, n.Name as 'Neighborhood Name' FROM Walker w LEFT JOIN Neighborhood n on w.NeighborhoodId = n.Id WHERE n.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Walker newWalker = new Walker();

                    if (reader.Read())
                    {
                        int idColumnPosition = reader.GetOrdinal("Id");
                        int IdValue = reader.GetInt32(idColumnPosition);

                        int NameColumnPosition = reader.GetOrdinal("Name");
                        string NameValue = reader.GetString(NameColumnPosition);

                        int neighborhoodIdColumnPosition = reader.GetOrdinal("neighborhoodId");
                        int neighborhoodIdValue = reader.GetInt32(neighborhoodIdColumnPosition);

                        newWalker = new Walker
                        {
                            Id = IdValue,
                            Name = NameValue,
                            NeighborhoodId = neighborhoodIdValue,
                            Neighborhood = null,
                            Walks = null
                        };
                    }

                    reader.Close();
                    return Ok(newWalker);
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