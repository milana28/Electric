namespace Electric.Models
{
    public class DeviceWithPosition
    {
        public int EnclosureId { set; get; }
        public Device Device { set; get; }
        public int Row { set; get; }
        public int Column { set; get; }
    }
}