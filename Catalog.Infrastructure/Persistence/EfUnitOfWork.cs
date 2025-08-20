using Catalog.Application.Abstractions;

namespace Catalog.Infrastructure.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly CatalogDbContext _db;
    public EfUnitOfWork(CatalogDbContext db) => _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}
