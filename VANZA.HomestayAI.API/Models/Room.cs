namespace VANZA.HomestayAI.API.Models
{
    public class Room
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; } = "";
        public string RoomType { get; set; } = "";
        public int Capacity { get; set; }
        public decimal RatePerNight { get; set; }
        public bool IsActive { get; set; } = true;
        public List<Booking> Bookings { get; set; } = new();
    }
}
