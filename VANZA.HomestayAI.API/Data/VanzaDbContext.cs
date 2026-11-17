
using Microsoft.EntityFrameworkCore;
using VANZA.HomestayAI.API.Models;

namespace VANZA.HomestayAI.API.Data
{
    public class VanzaDbContext : DbContext
    {
        public VanzaDbContext(DbContextOptions<VanzaDbContext> options): base(options)
        {
        }
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<Guest> Guests => Set<Guest>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<ChatConversation> ChatConversations => Set<ChatConversation>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // PRIMARY KEYS
            modelBuilder.Entity<Room>().HasKey(x => x.RoomId);
            modelBuilder.Entity<Guest>().HasKey(x => x.GuestId);
            modelBuilder.Entity<Booking>().HasKey(x => x.BookingId);
            modelBuilder.Entity<Payment>().HasKey(x => x.PaymentId);
            modelBuilder.Entity<ChatConversation>().HasKey(x => x.ChatId);
            modelBuilder.Entity<ChatMessage>().HasKey(x => x.MessageId);
            // DECIMAL PRECISION
            modelBuilder.Entity<Room>().Property(x => x.RatePerNight).HasPrecision(10, 2);
            modelBuilder.Entity<Booking>().Property(x => x.TotalAmount).HasPrecision(12, 2);
            modelBuilder.Entity<Booking>().Property(x => x.AdvancePaid).HasPrecision(12, 2);
            modelBuilder.Entity<Payment>().Property(x => x.Amount).HasPrecision(12, 2);
            // BOOKING -> GUEST
            modelBuilder.Entity<Booking>().HasOne(x => x.Guest).WithMany(x => x.Bookings).HasForeignKey(x => x.GuestId).OnDelete(DeleteBehavior.Restrict);
            // BOOKING -> ROOM
            modelBuilder.Entity<Booking>().HasOne(x => x.Room).WithMany(x => x.Bookings).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
            // PAYMENT -> BOOKING
            modelBuilder.Entity<Payment>().HasOne(x => x.Booking).WithMany(x => x.Payments).HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Restrict);
            // CHAT MESSAGE -> CHAT CONVERSATION
            modelBuilder.Entity<ChatMessage>().HasOne(x => x.Conversation).WithMany(x => x.Messages).HasForeignKey(x => x.ChatId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
