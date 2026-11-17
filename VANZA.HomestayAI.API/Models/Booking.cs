namespace VANZA.HomestayAI.API.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int GuestId { get; set; }
        public int RoomId { get; set; }

        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }

        public string BookingStatus { get; set; } = "";

        public decimal TotalAmount { get; set; }
        public decimal AdvancePaid { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public Guest? Guest { get; set; }
        public Room? Room { get; set; }

        public List<Payment> Payments { get; set; } = new();
    }
}
