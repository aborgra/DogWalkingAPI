using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalkingAPI.Models
{
    public class Owner
    {
        public int Id { get; set; }
        [Required]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Owner Name must be between 2 and 40 characters")]
        public string Name { get; set; }
        [Required]

        public int NeighborhoodId { get; set; }
        [Required]

        public string Address { get; set; }
        [RegularExpression("^[01]?[- .]?\\(?[2-9]\\d{2}\\)?[- .]?\\d{3}[- .]?\\d{4}$",
         ErrorMessage = "Phone is required and must be properly formatted. Ex: 555-555-5555")]

        public string Phone { get; set; }

        public Neighborhood Neighborhood { get; set; }

        public List<Dog> Dogs { get; set; }
    }
}
