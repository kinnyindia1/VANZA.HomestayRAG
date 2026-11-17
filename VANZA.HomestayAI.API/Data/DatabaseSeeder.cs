using Microsoft.EntityFrameworkCore;
using VANZA.HomestayAI.API.Models;

namespace VANZA.HomestayAI.API.Data
{
    public class DatabaseSeeder
    {
        public static async Task SeedAsync(VanzaDbContext db)
        {
            // Do not insert the demo data again.
            if (await db.Rooms.AnyAsync())
            {
                Console.WriteLine("[SEED] Sample data already exists. Skipping.");
                return;
            }

            // ROOMS
            var room1 = new Room
            {
                RoomName = "Double Room 101",
                RoomType = "Double",
                Capacity = 2,
                RatePerNight = 1500
            };

            var room2 = new Room
            {
                RoomName = "Double Room 102",
                RoomType = "Double",
                Capacity = 2,
                RatePerNight = 1800
            };

            var room3 = new Room
            {
                RoomName = "Family 2BHK",
                RoomType = "2BHK",
                Capacity = 5,
                RatePerNight = 3000
            };

            var room4 = new Room
            {
                RoomName = "Premium 2BHK",
                RoomType = "2BHK",
                Capacity = 5,
                RatePerNight = 3500
            };

            var room5 = new Room
            {
                RoomName = "Driver Dormitory",
                RoomType = "Dormitory",
                Capacity = 3,
                RatePerNight = 500
            };

            db.Rooms.AddRange(room1, room2, room3, room4, room5);

            // GUESTS
            var guest1 = new Guest
            {
                GuestName = "Arun Kumar",
                Phone = "9876543210",
                Email = "arun@example.com"
            };

            var guest2 = new Guest
            {
                GuestName = "Priya",
                Phone = "9876543211",
                Email = "priya@example.com"
            };

            var guest3 = new Guest
            {
                GuestName = "Ravi",
                Phone = "9876543212",
                Email = "ravi@example.com"
            };

            var guest4 = new Guest
            {
                GuestName = "Meena",
                Phone = "9876543213",
                Email = "meena@example.com"
            };

            db.Guests.AddRange(guest1, guest2, guest3, guest4);

            await db.SaveChangesAsync();

            // BOOKINGS
            var booking1 = new Booking
            {
                Guest = guest1,
                Room = room1,
                CheckIn = DateTime.Today.AddDays(-1),
                CheckOut = DateTime.Today.AddDays(1),
                BookingStatus = "CheckedIn",
                TotalAmount = 3000,
                AdvancePaid = 2000
            };

            var booking2 = new Booking
            {
                Guest = guest2,
                Room = room2,
                CheckIn = DateTime.Today,
                CheckOut = DateTime.Today.AddDays(1),
                BookingStatus = "Confirmed",
                TotalAmount = 2000,
                AdvancePaid = 1500
            };

            var booking3 = new Booking
            {
                Guest = guest3,
                Room = room3,
                CheckIn = DateTime.Today.AddDays(-3),
                CheckOut = DateTime.Today.AddDays(-1),
                BookingStatus = "Completed",
                TotalAmount = 4500,
                AdvancePaid = 1500
            };

            var booking4 = new Booking
            {
                Guest = guest4,
                Room = room4,
                CheckIn = DateTime.Today.AddDays(3),
                CheckOut = DateTime.Today.AddDays(5),
                BookingStatus = "Confirmed",
                TotalAmount = 6000,
                AdvancePaid = 2000
            };

            db.Bookings.AddRange(
                booking1, booking2, booking3, booking4);

            await db.SaveChangesAsync();

            // PAYMENTS
            db.Payments.AddRange(
                new Payment
                {
                    Booking = booking1,
                    Amount = 2000,
                    PaidAt = DateTime.Now,
                    PaymentStatus = "Completed",
                    PaymentMethod = "UPI"
                },
                new Payment
                {
                    Booking = booking2,
                    Amount = 1500,
                    PaidAt = DateTime.Now,
                    PaymentStatus = "Completed",
                    PaymentMethod = "Cash"
                }
            );

            await db.SaveChangesAsync();
            Console.WriteLine("[SEED] DATABASE SEEDING COMPLETED");
            Console.WriteLine($"Rooms: {await db.Rooms.CountAsync()}");
            Console.WriteLine($"Guests: {await db.Guests.CountAsync()}");
            Console.WriteLine($"Bookings: {await db.Bookings.CountAsync()}");
            Console.WriteLine($"Payments: {await db.Payments.CountAsync()}");
        }
    }
}
