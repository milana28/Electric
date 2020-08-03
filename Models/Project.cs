using System;
using System.Collections.Generic;

namespace Electric.Models
{
    public class Project
    {
        public int Id { set; get; }
        public string Name { set; get; }
        public DateTime Date { set; get; }
        public List<Enclosure> Enclosures { set; get; }
    }
}