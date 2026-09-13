using ConferenceBooking.Application.Services;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Infrastructure.Persistence;
using ConferenceBooking.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRoomService, RoomService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var contex = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    if (app.Environment.IsDevelopment())
    {
        try
        {
            await DatabaseSeeder.SeedAsync(contex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while applying migrations or populating the database");
            throw;
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
    //app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();