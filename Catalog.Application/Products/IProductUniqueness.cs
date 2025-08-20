namespace Catalog.Application.Products;

public interface IProductUniqueness
{
    Task<bool> ExistsBySku(string sku, CancellationToken ct);
}
