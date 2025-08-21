using Catalog.Application.Products;
using Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence.Products;

public sealed class ProductsWriter : IProductWriter
{
    private readonly CatalogDbContext _db;
    public ProductsWriter(CatalogDbContext db) => _db = db;

    public void Add(Product product, IEnumerable<Guid>? categoryIds = null)
    {
        _db.Add(product);
        if (categoryIds != null)
        {
            foreach (var categoryId in categoryIds)
            {
                _db.ProductCategories.Add(new ProductCategory(product.Id, categoryId));
            }
        }
    }

    public async Task Update(Product product, IEnumerable<Guid>? categoryIds, CancellationToken ct)
    {
        var existing = await _db.Products.Include(p => p.ProductCategories)
            .FirstOrDefaultAsync(p => p.Id == product.Id, ct);
        if (existing == null) throw new ArgumentException("Product not found", nameof(product.Id));
        existing.GetType().GetProperty("Sku")!.SetValue(existing, product.Sku);
        existing.GetType().GetProperty("Name")!.SetValue(existing, product.Name);
        existing.GetType().GetProperty("Description")!.SetValue(existing, product.Description);
        existing.GetType().GetProperty("ImageMainUrl")!.SetValue(existing, product.ImageMainUrl);
        // Update categories
        if (categoryIds != null)
        {
            _db.ProductCategories.RemoveRange(existing.ProductCategories);
            foreach (var categoryId in categoryIds)
            {
                _db.ProductCategories.Add(new ProductCategory(existing.Id, categoryId));
            }
        }
    }

    public async Task Delete(Guid productId, CancellationToken ct)
    {
        var product = await _db.Products.FindAsync(new object[] { productId }, ct);
        if (product == null) throw new ArgumentException("Product not found", nameof(productId));
        _db.Products.Remove(product);
    }
}
