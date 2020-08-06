namespace Electric.Models
{
    public class Enclosure_Device
    {
        public int Id { set; get; }
        public int EnclosureId { set; get; }
        public int DeviceId { set; get; }
        public int Rows { set; get; }
        public int Columns { set; get; }
    }
}