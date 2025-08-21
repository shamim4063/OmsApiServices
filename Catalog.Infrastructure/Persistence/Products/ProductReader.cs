using Catalog.Application.Products;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence.Products;

public sealed class ProductReader : IProductReader
{
    private readonly CatalogDbContext _db;
    public ProductReader(CatalogDbContext db) => _db = db;

    public Task<ProductDto?> ById(Guid id, CancellationToken ct) =>
        _db.Products
           .Where(p => p.Id == id)
           .Select(p => new ProductDto(p.Id, p.Sku, p.Name, p.IsActive))
           .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<ProductDto>> List(int skip, int take, CancellationToken ct) =>
        await _db.Products
                 .OrderBy(p => p.Sku)
                 .Skip(skip).Take(take)
                 .Select(p => new ProductDto(p.Id, p.Sku, p.Name, p.IsActive))
                 .ToListAsync(ct);
}
