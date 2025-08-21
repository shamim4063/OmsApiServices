namespace Catalog.Application.Products;

public record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    string? ImageMainUrl,
    bool IsActive,
    IReadOnlyList<Guid> CategoryIds
);
