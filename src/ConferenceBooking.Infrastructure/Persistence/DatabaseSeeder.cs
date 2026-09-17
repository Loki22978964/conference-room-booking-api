using ConferenceBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
        }

        if (!await context.Rooms.AnyAsync())
        {
            var roomA = new Room("Hall A", 50, 2000m);
            var roomB = new Room("Hall B", 100, 3500m);
            var roomC = new Room("Hall C", 30, 1500m);

            context.Rooms.AddRange(roomA, roomB, roomC);

            var projector = new Service("Projector", 500m);
            var wifi = new Service("Wi-Fi", 300m);
            var sound = new Service("Sound", 700m);

            context.Services.AddRange(projector, wifi, sound);
            await context.SaveChangesAsync();

            var rooms = new[] { roomA, roomB, roomC };
            var services = new[] { projector, wifi, sound };

            foreach (var r in rooms)
            {
                foreach (var s in services)
                {
                    context.RoomServices.Add(new RoomService(r.Id, s.Id));
                }
            }

            await context.SaveChangesAsync();
        }
    }
}