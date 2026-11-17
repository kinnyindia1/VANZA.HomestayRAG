namespace VANZA.HomestayAI.API.Models
{
    public class Guest
    {
        public int GuestId { get; set; }
        public string GuestName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<Booking> Bookings { get; set; } = new();
    }
}
