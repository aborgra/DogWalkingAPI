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
    public class DogController : ControllerBase
    {
       

            private readonly IConfiguration _config;

            public DogController(IConfiguration config)
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
                List<Dog> dogs = new List<Dog>();

                cmd.CommandText = @"SELECT d.Id, d.Name, d.OwnerId, d.Breed, d.Notes, o.Id as 'Owner Id', o.Name as 'Owner Name', o.NeighborhoodId 
                                    FROM Dog d 
                                    LEFT JOIN Owner o 
                                    on d.OwnerId = o.Id";
                SqlDataReader reader = cmd.ExecuteReader();

                Dog newDog = null;
                while (reader.Read())
                {

                    int idColumnPosition = reader.GetOrdinal("Id");
                    int idValue = reader.GetInt32(idColumnPosition);

                    int nameColumnPosition = reader.GetOrdinal("Name");
                    string nameValue = reader.GetString(nameColumnPosition);

                    int ownerIdColumnPosition = reader.GetOrdinal("OwnerId");
                    int ownerIdValue = reader.GetInt32(ownerIdColumnPosition);

                    int breedColumnPosition = reader.GetOrdinal("Breed");
                    string breedValue = reader.GetString(breedColumnPosition);

                    int notesColumnPosition = reader.GetOrdinal("Notes");
                    string notesValue = reader.GetString(notesColumnPosition);

                    int oIdColumnPosition = reader.GetOrdinal("Owner Id");
                    int oIdValue = reader.GetInt32(oIdColumnPosition);

                    int ownerColumnPosition = reader.GetOrdinal("Owner Name");
                    string ownerValue = reader.GetString(ownerColumnPosition);

                    int nIdColumnPosition = reader.GetOrdinal("NeighborhoodId");
                    int nIdValue = reader.GetInt32(nIdColumnPosition);

                    newDog = new Dog
                    {
                        Name = nameValue,
                        Id = idValue,
                        OwnerId = ownerIdValue,
                        Breed = breedValue,
                        Notes = notesValue,
                        Owner = new Owner()
                        {
                            Id = oIdValue,
                            Name = ownerValue,
                            NeighborhoodId = nIdValue
                        }

                    };
                    dogs.Add(newDog);
                }
                reader.Close();
                return Ok(dogs);
            }
        }


        [HttpGet("{id}", Name = "GetDog")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT d.Id, d.Name, d.OwnerId, d.Breed, d.Notes, o.Id as 'Owner Id', o.Name as 'Owner Name', o.NeighborhoodId FROM Dog d LEFT JOIN Owner o on d.OwnerId = o.Id WHERE d.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Dog newDog = null;

                    if (reader.Read())
                    {
                        int idColumnPosition = reader.GetOrdinal("Id");
                        int idValue = reader.GetInt32(idColumnPosition);

                        int nameColumnPosition = reader.GetOrdinal("Name");
                        string nameValue = reader.GetString(nameColumnPosition);

                        int ownerIdColumnPosition = reader.GetOrdinal("OwnerId");
                        int ownerIdValue = reader.GetInt32(ownerIdColumnPosition);

                        int breedColumnPosition = reader.GetOrdinal("Breed");
                        string breedValue = reader.GetString(breedColumnPosition);

                        int notesColumnPosition = reader.GetOrdinal("Notes");
                        string notesValue = reader.GetString(notesColumnPosition);

                        int oIdColumnPosition = reader.GetOrdinal("Owner Id");
                        int oIdValue = reader.GetInt32(oIdColumnPosition);

                        int ownerColumnPosition = reader.GetOrdinal("Owner Name");
                        string ownerValue = reader.GetString(ownerColumnPosition);

                        int nIdColumnPosition = reader.GetOrdinal("NeighborhoodId");
                        int nIdValue = reader.GetInt32(nIdColumnPosition);

                        newDog = new Dog
                        {
                            Name = nameValue,
                            Id = idValue,
                            OwnerId = ownerIdValue,
                            Breed = breedValue,
                            Notes = notesValue,
                            Owner = new Owner()
                            {
                                Id = oIdValue,
                                Name = ownerValue,
                                NeighborhoodId = nIdValue
                            }

                        };
                        
                    }
                    reader.Close();
                    return Ok(newDog);
                }
            }
        }

            [HttpPost]
            public async Task<IActionResult> Post([FromBody] Dog dog)
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "INSERT INTO Dog (Name, OwnerId,Breed,Notes) OUTPUT INSERTED.Id Values (@Name, @OwnerId, @Breed, @Notes)";
                        cmd.Parameters.Add(new SqlParameter("@Name", dog.Name));
                        cmd.Parameters.Add(new SqlParameter("@OwnerId", dog.@OwnerId));
                        cmd.Parameters.Add(new SqlParameter("@Breed", dog.Breed));
                        cmd.Parameters.Add(new SqlParameter("@Notes", dog.Notes));

                        int newId = (int)cmd.ExecuteScalar();

                        dog.Id = newId;
                        return CreatedAtRoute("GetDog", new { id = newId }, dog);
                    }
                }
            }

            [HttpPut("{id}")]
            public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Dog dog)
            {
                try
                {
                    using (SqlConnection conn = Connection)
                    {
                        conn.Open();
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"UPDATE Dog
                                     SET Notes = @notes
                                     WHERE Id = @id";
                            cmd.Parameters.Add(new SqlParameter("@notes", dog.Notes));
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
                    if (!DogExists(id))
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
                            cmd.CommandText = @"DELETE FROM Dog WHERE Id = @id";
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
                    if (!DogExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            private bool DogExists(int id)
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT Id, Name, OwnerId, Breed, Notes FROM Dog ";

                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        SqlDataReader reader = cmd.ExecuteReader();
                        return reader.Read();
                    }
                }
            }

        }
    
}