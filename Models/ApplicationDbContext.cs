using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        // =====================================
        // TABLES
        // =====================================

        public DbSet<User> Users { get; set; }

        public DbSet<Car> Cars { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<UserVerification> UserVerifications { get; set; }

        // =====================================
        // RELATIONSHIPS
        // =====================================

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------------------------------
            // CAR COLUMN PRECISION
            // ---------------------------------
            modelBuilder.Entity<Car>(entity =>
            {
                entity.Property(e => e.PricePerDay).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Latitude).HasColumnType("decimal(10,7)");
                entity.Property(e => e.Longitude).HasColumnType("decimal(10,7)");
            });

            // ---------------------------------
            // BOOKING COLUMN PRECISION
            // ---------------------------------
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Latitude).HasColumnType("decimal(10,7)");
                entity.Property(e => e.Longitude).HasColumnType("decimal(10,7)");
            });

            // ---------------------------------
            // BOOKING -> USER
            // ---------------------------------

            modelBuilder.Entity<Booking>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------------------------------
            // BOOKING -> CAR
            // ---------------------------------

            modelBuilder.Entity<Booking>()
                .HasOne(x => x.Car)
                .WithMany()
                .HasForeignKey(x => x.CarId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------------------------------
            // VERIFICATION -> USER
            // ---------------------------------

            modelBuilder.Entity<UserVerification>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}