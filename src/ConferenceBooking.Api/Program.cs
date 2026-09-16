using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Application.Services;
using ConferenceBooking.Infrastructure.Persistence;
using ConferenceBooking.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

if (Environment.GetEnvironmentVariable("POSTGRES_HOST") is null)
{
    Env.TraversePath().Load();
}

var builder = WebApplication.CreateBuilder(args);

var dbHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
var dbPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
var dbName = Environment.GetEnvironmentVariable("POSTGRES_DB");
var dbUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
var dbPass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPass}";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
    else
    {
        Console.WriteLine($"[SWAGGER WARNING] XML documentation file not found at: {xmlPath}");
    }
});

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
}



app.MapControllers();

await app.RunAsync();