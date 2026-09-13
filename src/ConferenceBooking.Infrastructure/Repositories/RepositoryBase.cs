using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using ConferenceBooking.Domain.Entities;
using ConferenceBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Infrastructure.Repositories;

public abstract class RepositoryBase<T> where T : BaseEntity
{
    protected readonly AppDbContext DbContext;

    protected RepositoryBase(AppDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public async Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).CountAsync(cancellationToken);
    }

    public void Add(T entity) => DbContext.Set<T>().Add(entity);

    public void Remove(T entity) => DbContext.Set<T>().Remove(entity);

    private IQueryable<T> ApplySpecification(ISpecification<T> specification)
        => SpecificationEvaluator.Default.GetQuery(DbContext.Set<T>().AsQueryable(), specification);
}