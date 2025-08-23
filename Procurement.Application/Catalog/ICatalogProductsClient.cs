namespace Procurement.Application.Catalog;

public interface ICatalogProductsClient
{
    Task<IReadOnlyDictionary<Guid, ProductDto>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct);
}

public sealed record ProductDto(
    Guid Id, string Sku, string Name, string? Description, string? ImageMainUrl);

