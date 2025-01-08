using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using MTM_Web_App.Server.Controllers;
using MTM_Web_App.Server.Models;

namespace MTM_Web_App.Server.Data
{
    public class MTM_Web_AppServerContext(DbContextOptions<MTM_Web_AppServerContext> options) : DbContext(options)
    {
        public DbSet<Address> Addresses { get; set; } = default!;
        public DbSet<Hotel> Hotels { get; set; } = default!;
        public DbSet<HotelRes> HotelRes { get; set; } = default!;
        public DbSet<Room> Rooms { get; set; } = default!;
        public DbSet<RoomEntity> RoomEntities { get; set; } = default!;
        public DbSet<Restaurant> Restaurants { get; set; } = default!;
        public DbSet<RestaurantRes> RestaurantsRes { get; set; } = default!;
        public DbSet<Table> Tables { get; set; } = default!;
        public DbSet<TableEntity> TableEntities { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Logger> Logger { get; set; } = default!;
        public DbSet<OTP> OTPs { get; set; } = default!;
        public DbSet<HRating> HotelRatings { get; set; } = default!;
        public DbSet<RRating> RestaurantRating { get; set; } = default!;
        public DbSet<HotelResProcessed> HotelResProcessed { get; set; }
        public DbSet<HotelResProcessed> RestaurantResProcessed { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.HasOne(a => a.Hotel)
                      .WithMany(h => h.Addresses)
                      .HasForeignKey(a => a.HotelId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Restaurant)
                      .WithMany(r => r.Addresses)
                      .HasForeignKey(a => a.RestaurantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<HotelRes>()
                .HasOne(hr => hr.ClientUser)
                .WithMany(u => u.HotelRes)
                .HasForeignKey(hr => hr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HotelRes>()
                .HasOne(hr => hr.Room)
                .WithMany(h => h.HotelReservations)
                .HasForeignKey(hr => hr.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RestaurantRes>()
                .HasOne(rr => rr.ClientUser)
                .WithMany(u => u.RestaurantRes)
                .HasForeignKey(rr => rr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RestaurantRes>()
                .HasOne(rr => rr.Table)
                .WithMany(r => r.RestaurantReservations)
                .HasForeignKey(rr => rr.TableId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Restaurant>()
                .HasMany(r => r.OpenDays)
                .WithOne(oh => oh.Restaurant)
                .HasForeignKey(oh => oh.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HotelRes>()
                .Property(h => h.SummaryCost)
                .HasPrecision(18, 2);  // Określa precyzję 18 cyfr i 2 miejsca po przecinku

            modelBuilder.Entity<RestaurantRes>()
                .Property(r => r.SummaryCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Room>()
                .Property(r => r.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Table>()
                .Property(t => t.Price)
                .HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);
        }
    }
}
