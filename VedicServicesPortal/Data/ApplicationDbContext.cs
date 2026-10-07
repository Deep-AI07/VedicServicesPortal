using Microsoft.EntityFrameworkCore;
using VedicServicesPortal.Models;

namespace VedicServicesPortal.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ==========================================
        // TABLES
        // ==========================================

        public DbSet<User> Users { get; set; }

        public DbSet<Pandit> Pandits { get; set; }

        public DbSet<Service> Services { get; set; }

        public DbSet<TimeSlot> TimeSlots { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<SiteSetting> SiteSettings { get; set; }


        // ==========================================
        // DATABASE RELATIONSHIPS
        // ==========================================

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // ==========================================
            // BOOKING → USER
            // ==========================================

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // BOOKING → PANDIT
            // ==========================================

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Pandit)
                .WithMany(p => p.Bookings)
                .HasForeignKey(b => b.PanditId)
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // BOOKING → SERVICE
            // ==========================================

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Service)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // BOOKING → TIME SLOT
            // ==========================================

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.TimeSlot)
                .WithMany()
                .HasForeignKey(b => b.SlotId)
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // TIME SLOT → PANDIT
            // ==========================================

            modelBuilder.Entity<TimeSlot>()
                .HasOne(t => t.Pandit)
                .WithMany(p => p.TimeSlots)
                .HasForeignKey(t => t.PanditId)
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // DECIMAL PRECISION
            // ==========================================

            modelBuilder.Entity<Service>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalAmount)
                .HasColumnType("decimal(18,2)");
        }
    }
}
