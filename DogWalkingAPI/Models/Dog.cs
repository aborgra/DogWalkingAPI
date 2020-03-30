﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalkingAPI.Models
{
    public class Dog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OwnerId { get; set; }
        public string Breed { get; set; }
        public string Notes { get; set; }

        public Owner Owner { get; set; }

    }
}