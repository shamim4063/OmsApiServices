using Catalog.Application.Products;
using Catalog.Domain;

namespace Catalog.Infrastructure.Persistence.Products;

public sealed class ProductsWriter : IProductsWriter
{
    private readonly CatalogDbContext _db;
    public ProductsWriter(CatalogDbContext db) => _db = db;

    public void Add(Product product) => _db.Add(product);
}
