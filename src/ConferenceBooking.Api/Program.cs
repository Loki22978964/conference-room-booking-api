using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Application.Services;
using ConferenceBooking.Infrastructure.Persistence;
using ConferenceBooking.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IReportService, ReportService>();

//builder.Services.AddSwaggerGen(options =>
//{
//    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
//    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
//    options.IncludeXmlComments(xmlPath);
//});

builder.Services.AddSwaggerGen(options =>
{
    // Отримуємо назву поточної збірки (ConferenceBooking.Api.xml)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        // Другий аргумент true вмикає коментарі для самого контролера/тегів
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
    //app.UseHttpsRedirection();
}



app.MapControllers();

app.Run();