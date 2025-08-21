using MediatR;
using Catalog.Application.Abstractions;

namespace Catalog.Application.Products;

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

public class CreateProductHandler : IRequestHandler<CreateProduct, Guid>
{
    private readonly IProductUniqueness _uniqueness;
    private readonly IProductWriter _writer;
    private readonly IUnitOfWork _uow;

    public CreateProductHandler(
        IProductUniqueness uniqueness,
        IProductWriter writer,
        IUnitOfWork uow)
    {
        _uniqueness = uniqueness;
        _writer = writer;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateProduct cmd, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cmd.Sku))
            throw new ArgumentException("SKU is required", nameof(cmd.Sku));
        if (string.IsNullOrWhiteSpace(cmd.Name))
            throw new ArgumentException("Name is required", nameof(cmd.Name));

        if (await _uniqueness.ExistsBySku(cmd.Sku.Trim(), ct))
            throw new InvalidOperationException("SKU already exists");

        var product = new Domain.Product(cmd.Sku.Trim(), cmd.Name.Trim(), cmd.Description, cmd.ImageMainUrl);
        _writer.Add(product, cmd.CategoryIds);
        await _uow.SaveChangesAsync(ct);
        return product.Id;
    }
}

public class UpdateProductHandler : IRequestHandler<UpdateProduct>
{
    private readonly IProductWriter _writer;
    private readonly IUnitOfWork _uow;
    public UpdateProductHandler(IProductWriter writer, IUnitOfWork uow)
    {
        _writer = writer;
        _uow = uow;
    }
    public async Task Handle(UpdateProduct cmd, CancellationToken ct)
    {
        var product = new Domain.Product(cmd.Sku, cmd.Name, cmd.Description, cmd.ImageMainUrl);
        typeof(Domain.Product).GetProperty("Id")!.SetValue(product, cmd.Id);
        await _writer.Update(product, cmd.CategoryIds, ct);
        await _uow.SaveChangesAsync(ct);
    }
}

public class DeleteProductHandler : IRequestHandler<DeleteProduct>
{
    private readonly IProductWriter _writer;
    private readonly IUnitOfWork _uow;
    public DeleteProductHandler(IProductWriter writer, IUnitOfWork uow)
    {
        _writer = writer;
        _uow = uow;
    }
    public async Task Handle(DeleteProduct cmd, CancellationToken ct)
    {
        await _writer.Delete(cmd.Id, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
