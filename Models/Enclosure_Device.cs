namespace Electric.Models
{
    public class Enclosure_Device
    {
        public int Id { set; get; }
        public int EnclosureId { set; get; }
        public int DeviceId { set; get; }
        public int Row { set; get; }
        public int Column { set; get; }
    }
}