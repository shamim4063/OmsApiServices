using MediatR;

namespace Catalog.Application.Products;

public sealed record GetProductById(Guid Id) : IRequest<ProductDto?>;
public sealed record ListProducts(int Skip, int Take) : IRequest<IReadOnlyList<ProductDto>>;
public sealed record CreateProduct(
    string Sku,
    string Name,
    string? Description,
    string? ImageMainUrl,
    List<Guid> CategoryIds
) : IRequest<Guid>;
public sealed record GetProductsByIds(List<Guid> Ids) : IRequest<List<ProductDto>>;

public sealed record UpdateProduct(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    string? ImageMainUrl,
    List<Guid> CategoryIds
) : IRequest;
public sealed record DeleteProduct(Guid Id) : IRequest;
