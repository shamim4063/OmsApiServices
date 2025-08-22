using Procurement.Application.Abstractions;

namespace Procurement.Infrastructure;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly ProcurementDbContext _db;
    public EfUnitOfWork(ProcurementDbContext db) => _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}
