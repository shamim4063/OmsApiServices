namespace Catalog.Application.Products;

public interface IProductReader
{
    Task<ProductDto?> ById(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ProductDto>> List(int skip, int take, CancellationToken ct);
}
