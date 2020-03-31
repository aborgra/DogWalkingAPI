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
    public class OwnerController : ControllerBase
    {

        private readonly IConfiguration _config;

        public OwnerController(IConfiguration config)
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
        public async Task<IActionResult> Get([FromQuery] string include, [FromQuery] string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using SqlCommand cmd = conn.CreateCommand();
                {
                var includeQuery = "";
                var joinQuery = "";
                if (include == "neighborhood")
                {
                    includeQuery = " n.Name as 'Neighborhood Name', ";
                    joinQuery = " LEFT JOIN Neighborhood N ON o.NeighborhoodId = n.Id ";

                }
                cmd.CommandText = $@"SELECT o.Id, o.Name, o.NeighborhoodId, o.Phone, o.Address, 
                                    {includeQuery}d.Id as 'Dog Id', d.Name as 'Dog Name', d.breed, d.notes, d.OwnerId as 'Owner Id' 
                                    FROM Owner o 
                                    {joinQuery} 
                                    LEFT JOIN Dog d on o.id = d.OwnerId";
                if (q != null)
                {
                    cmd.CommandText += " WHERE o.Name LIKE @Name";
                    cmd.Parameters.Add(new SqlParameter("@Name", "%" + q + "%"));
                }

                SqlDataReader reader = cmd.ExecuteReader();

                List<Owner> owners = new List<Owner>();


                while (reader.Read())
                {
                    int idValue = reader.GetInt32(reader.GetOrdinal("Id"));
                    var existingOwner = owners.FirstOrDefault(o => o.Id == idValue);

                    if (include == "neighborhood")
                    {

                        if (existingOwner == null)
                        {

                            Owner owner = new Owner()
                            {
                                Id = idValue,
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                Neighborhood = new Neighborhood
                                {
                                    Name = reader.GetString(reader.GetOrdinal("Neighborhood Name")),
                                    Id = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"))
                                },
                                Dogs = new List<Dog>()

                            };
                            owner.Dogs.Add(new Dog()
                            {
                                Name = reader.GetString(reader.GetOrdinal("Dog Name")),
                                Id = reader.GetInt32(reader.GetOrdinal("Dog Id")),
                                Breed = reader.GetString(reader.GetOrdinal("Breed")),
                                Notes = reader.GetString(reader.GetOrdinal("Notes")),
                                OwnerId = reader.GetInt32(reader.GetOrdinal("Owner Id"))

                            });

                            owners.Add(owner);
                        }
                        else
                        {

                            existingOwner.Dogs.Add(new Dog()
                            {
                                Name = reader.GetString(reader.GetOrdinal("Dog Name")),
                                Id = reader.GetInt32(reader.GetOrdinal("Dog Id")),
                                Breed = reader.GetString(reader.GetOrdinal("Breed")),
                                Notes = reader.GetString(reader.GetOrdinal("Notes")),
                                OwnerId = reader.GetInt32(reader.GetOrdinal("Owner Id"))

                            });
                        }
                    }
                    else
                    {
                        if (existingOwner == null)
                        {

                            Owner owner = new Owner()
                            {
                                Id = idValue,
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                Dogs = new List<Dog>()

                            };
                            owner.Dogs.Add(new Dog()
                            {
                                Name = reader.GetString(reader.GetOrdinal("Dog Name")),
                                Id = reader.GetInt32(reader.GetOrdinal("Dog Id")),
                                Breed = reader.GetString(reader.GetOrdinal("Breed")),
                                Notes = reader.GetString(reader.GetOrdinal("Notes")),
                                OwnerId = reader.GetInt32(reader.GetOrdinal("Owner Id"))

                            });

                            owners.Add(owner);
                        }
                        else
                        {

                            existingOwner.Dogs.Add(new Dog()
                            {
                                Name = reader.GetString(reader.GetOrdinal("Dog Name")),
                                Id = reader.GetInt32(reader.GetOrdinal("Dog Id")),
                                Breed = reader.GetString(reader.GetOrdinal("Breed")),
                                Notes = reader.GetString(reader.GetOrdinal("Notes")),
                                OwnerId = reader.GetInt32(reader.GetOrdinal("Owner Id"))

                            });
                        }
                    }

                }

                reader.Close();
                return Ok(owners);
            }
        }
        }
        

        [HttpGet("{id}", Name = "GetOwner")]
        public async Task<IActionResult> Get([FromRoute] int id, [FromQuery] string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    var paramQuery = "";
                    var fromQuery = "";
                    if (include == "neighborhood")
                    {
                        paramQuery = " n.Name NeighborhoodName,";
                        fromQuery = "Left Join  Neighborhood n On o.NeighborhoodId = n.Id";
                    }
                    cmd.CommandText = $@"SELECT o.Id, o.Name, o.Address, o.NeighborhoodId, o.Phone,{paramQuery} d.Id DogId ,d.Name DogName, d.Breed, d.Notes, d.OwnerId
                        FROM Owner o
                        {fromQuery}
                        Left Join Dog d
                        On o.Id = d.OwnerId
                        Where o.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Owner newOwner = null;

                    while (reader.Read())
                    {
                        if(newOwner == null)
                        {
                            int idColumnPosition = reader.GetOrdinal("Id");
                            int IdValue = reader.GetInt32(idColumnPosition);

                            int NameColumnPosition = reader.GetOrdinal("Name");
                            string NameValue = reader.GetString(NameColumnPosition);

                            int neighborhoodColumnPosition = reader.GetOrdinal("Neighborhood Name");
                            string neighborhoodValue = reader.GetString(neighborhoodColumnPosition);

                            int phoneColumnPosition = reader.GetOrdinal("Phone");
                            string phoneValue = reader.GetString(phoneColumnPosition);

                            int addressColumnPosition = reader.GetOrdinal("Address");
                            string addressValue = reader.GetString(addressColumnPosition);

                            int neighborhoodIdColumnPosition = reader.GetOrdinal("neighborhoodId");
                            int neighborhoodIdValue = reader.GetInt32(neighborhoodIdColumnPosition);

                            newOwner = new Owner
                            {
                                Id = IdValue,
                                Name = NameValue,
                                Phone = phoneValue,
                                Address = addressValue,
                                NeighborhoodId = neighborhoodIdValue,
                                Neighborhood = new Neighborhood
                                {
                                    Name = neighborhoodValue,
                                    Id = neighborhoodIdValue
                                },
                                Dogs = new List<Dog>()
                                
                            };
                        }
                        newOwner.Dogs.Add(new Dog()
                        {
                            Name = reader.GetString(reader.GetOrdinal("Dog Name")),
                            Id = reader.GetInt32(reader.GetOrdinal("Dog Id")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            Notes = reader.GetString(reader.GetOrdinal("Notes")),
                            OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId"))

                        });

                    }

                    reader.Close();
                    return Ok(newOwner);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Owner owner)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Owner (Name, NeighborhoodId, Address, Phone) OUTPUT INSERTED.Id Values (@Name, @NeighborhoodId, @Address, @Phone)";
                    cmd.Parameters.Add(new SqlParameter("@Name", owner.Name));
                    cmd.Parameters.Add(new SqlParameter("@NeighborhoodId", owner.NeighborhoodId));
                    cmd.Parameters.Add(new SqlParameter("@Phone", owner.Phone));
                    cmd.Parameters.Add(new SqlParameter("@Address", owner.Address));

                    int newId = (int)cmd.ExecuteScalar();

                    owner.Id = newId;
                    return CreatedAtRoute("GetOwner", new { id = newId }, owner);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Owner owner)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Owner
                                     SET NeighborhoodId = @neighborhoodId
                                     WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@neighborhoodId", owner.NeighborhoodId));
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
                if (!OwnerExists(id))
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
                        cmd.CommandText = @"DELETE FROM Owner WHERE Id = @id";
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
                if (!OwnerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool OwnerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, Address, NeighborhoodId FROM Owner ";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }

    }
}