using Catalog.Domain;
using Catalog.Application.Abstractions;

namespace Catalog.Application.Products;

public sealed record CreateProduct(string Sku, string Name);

public sealed class CreateProductHandler
{
    private readonly IProductUniqueness _uniqueness;
    private readonly IProductsWriter _writer;
    private readonly IUnitOfWork _uow;

    public CreateProductHandler(
        IProductUniqueness uniqueness,
        IProductsWriter writer,
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

        var product = new Product(cmd.Sku.Trim(), cmd.Name.Trim());
        _writer.Add(product);

        await _uow.SaveChangesAsync(ct);
        return product.Id;
    }
}
