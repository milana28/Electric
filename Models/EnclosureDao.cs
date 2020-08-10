using System;

namespace Electric.Models
{
    public class EnclosureDao
    {
        public int Id { set; get; }
        public string Name { set; get; }
        public DateTime Date { set; get; }
        public int ProjectId { set; get; }
        public float? TotalPrice { set; get; }
    }
}