using ConferenceBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConferenceBooking.UnitTests.Application.Specifications;

public abstract class SpecificationTestBase
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_dbName)
            .Options;

        return new AppDbContext(options);
    }
}