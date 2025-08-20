using Catalog.Application.Products;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

public sealed class ProductUniqueness : IProductUniqueness
{
    private readonly CatalogDbContext _db;
    public ProductUniqueness(CatalogDbContext db) => _db = db;

    public Task<bool> ExistsBySku(string sku, CancellationToken ct) =>
        _db.Products.AnyAsync(p => p.Sku == sku, ct);
}
