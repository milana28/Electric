using System;
using System.Collections.Generic;

namespace Electric.Models
{
    public class Enclosure
    {
        public int Id { set; get; }
        public string Name { set; get; }
        public DateTime Date { set; get; }
        public int ProjectId { set; get; }
        public List<Device> Devices { set; get; }
        public float? TotalPrice { set; get; }
        public EnclosureSpecs EnclosureSpecs { set; get; }
    }
}