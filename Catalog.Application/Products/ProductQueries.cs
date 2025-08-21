using MediatR;

namespace Catalog.Application.Products;

public sealed record GetProductById(Guid Id) : IRequest<ProductDto?>;
public sealed record ListProducts(int Skip, int Take) : IRequest<IReadOnlyList<ProductDto>>;

public class GetProductByIdHandler : IRequestHandler<GetProductById, ProductDto?>
{
    private readonly IProductReader _reader;
    public GetProductByIdHandler(IProductReader reader) => _reader = reader;
    public Task<ProductDto?> Handle(GetProductById request, CancellationToken cancellationToken)
        => _reader.ById(request.Id, cancellationToken);
}

public class ListProductsHandler : IRequestHandler<ListProducts, IReadOnlyList<ProductDto>>
{
    private readonly IProductReader _reader;
    public ListProductsHandler(IProductReader reader) => _reader = reader;
    public Task<IReadOnlyList<ProductDto>> Handle(ListProducts request, CancellationToken cancellationToken)
        => _reader.List(request.Skip, request.Take, cancellationToken);
}
