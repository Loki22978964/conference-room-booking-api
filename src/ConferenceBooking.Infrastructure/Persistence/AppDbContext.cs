using ConferenceBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<RoomService> RoomServices => Set<RoomService>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingService> BookingServices => Set<BookingService>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Дозволяємо використання GiST індексів у PostgreSQL
        modelBuilder.HasPostgresExtension("btree_gist");

        modelBuilder.Entity<Room>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.Name).HasMaxLength(100).IsRequired();
            b.Property(r => r.BaseHourlyRate).HasPrecision(12, 2);
            b.HasQueryFilter(r => r.IsActive); // Завжди ігнорувати видалені зали
        });

        modelBuilder.Entity<Service>(b =>
        {
            b.HasKey(s => s.Id);
            b.Property(s => s.Name).HasMaxLength(100).IsRequired();
            b.Property(s => s.Price).HasPrecision(12, 2);
        });

        modelBuilder.Entity<RoomService>(b =>
        {
            b.HasKey(rs => new { rs.RoomId, rs.ServiceId });
        });

        modelBuilder.Entity<Booking>(b =>
        {
            b.HasKey(bk => bk.Id);
            b.Property(bk => bk.HourlyRateSnapshot).HasPrecision(12, 2);
            b.Property(bk => bk.TotalPrice).HasPrecision(12, 2);

            // Налаштуємо constraints пізніше у самій міграції
        });

        modelBuilder.Entity<BookingService>(b =>
        {
            b.HasKey(bs => new { bs.BookingId, bs.ServiceId });
            b.Property(bs => bs.PriceSnapshot).HasPrecision(12, 2);
        });
    }
}
