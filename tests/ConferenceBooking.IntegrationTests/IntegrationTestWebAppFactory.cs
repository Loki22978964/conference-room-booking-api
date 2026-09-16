using ConferenceBooking.Domain.Entities;
using ConferenceBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.PostgreSql;

namespace ConferenceBooking.IntegrationTests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("conference_db_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" пропускає dev-only блок автосіду в Program.cs,
        // щоб кожен тестовий прогін починався з чистої, тільки-мігрованої бази.
        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        Environment.SetEnvironmentVariable("POSTGRES_HOST", _dbContainer.Hostname);
        Environment.SetEnvironmentVariable("POSTGRES_PORT", _dbContainer.GetMappedPublicPort(5432).ToString());
        Environment.SetEnvironmentVariable("POSTGRES_DB", "conference_db_test");
        Environment.SetEnvironmentVariable("POSTGRES_USER", "postgres");
        Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", "postgres");

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}

[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>
{
}
