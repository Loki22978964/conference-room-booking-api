using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.UnitTests.Application.Specifications;

internal static class EfQueryExtensions
{
    public static Task<T?> FirstOrDefaultSafeAsync<T>(
        this IQueryable<T> source, CancellationToken cancellationToken = default)
        => Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstOrDefaultAsync(source, cancellationToken);

    public static Task<List<T>> ToListSafeAsync<T>(
        this IQueryable<T> source, CancellationToken cancellationToken = default)
        => Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .ToListAsync(source, cancellationToken);
}
