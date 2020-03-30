﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalkingAPI.Models
{
    public class Walker
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int NeighborhoodId { get; set; }

        public Neighborhood Neighborhood { get; set; }

        public List<Walk> Walks = new List<Walk>();

    }
}