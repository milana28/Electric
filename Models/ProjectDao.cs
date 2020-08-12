using System;

namespace Electric.Models
{
    public class ProjectDao
    {
        public int Id { set; get; }
        public string Name { set; get; }
        public DateTime Date { set; get; }
        public DateTime? UpdateDate { set; get; }
    }
}