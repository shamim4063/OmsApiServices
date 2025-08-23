using Catalog.Domain;

namespace Catalog.Application.Products;

public interface IProductReader
{
    Task<ProductDto?> ById(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ProductDto>> List(int skip, int take, CancellationToken ct);
    Task<List<ProductDto>> ByIds(List<Guid> ids, CancellationToken ct);
}

public interface IProductWriter
{
    void Add(Product product, IEnumerable<Guid>? categoryIds = null);
    Task Update(Product product, IEnumerable<Guid>? categoryIds, CancellationToken ct);
    Task Delete(Guid productId, CancellationToken ct);
}
