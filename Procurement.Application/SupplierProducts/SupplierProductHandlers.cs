using MediatR;
using Procurement.Application.Suppliers;
using System.Net.Http;
using System.Net.Http.Json;

namespace Procurement.Application.SupplierProducts;

public class GetSupplierProductByIdHandler : IRequestHandler<GetSupplierProductById, SupplierProductDto?>
{
    private readonly ISupplierProductReader _reader;
    public GetSupplierProductByIdHandler(ISupplierProductReader reader) => _reader = reader;
    public Task<SupplierProductDto?> Handle(GetSupplierProductById request, CancellationToken ct)
        => _reader.ById(request.SupplierId, request.ProductId, ct);
}

public class ListSupplierProductsBySupplierHandler : IRequestHandler<ListSupplierProductsBySupplier, IReadOnlyList<SupplierProductDto>>
{
    private readonly ISupplierProductReader _reader;
    public ListSupplierProductsBySupplierHandler(ISupplierProductReader reader) => _reader = reader;
    public Task<IReadOnlyList<SupplierProductDto>> Handle(ListSupplierProductsBySupplier request, CancellationToken ct)
        => _reader.ListBySupplier(request.SupplierId, ct);
}

public class ListSupplierProductsHandler : IRequestHandler<ListSupplierProducts, IReadOnlyList<SupplierProductDto>>
{
    private readonly ISupplierProductReader _reader;
    public ListSupplierProductsHandler(ISupplierProductReader reader) => _reader = reader;
    public Task<IReadOnlyList<SupplierProductDto>> Handle(ListSupplierProducts request, CancellationToken ct)
        => _reader.List(request.Skip, request.Take, ct);
}

public class CreateSupplierProductHandler : IRequestHandler<CreateSupplierProduct>
{
    private readonly ISupplierProductWriter _writer;
    public CreateSupplierProductHandler(ISupplierProductWriter writer) => _writer = writer;
    public Task Handle(CreateSupplierProduct cmd, CancellationToken ct)
        => _writer.Add(cmd.Dto, ct);
}

public class UpdateSupplierProductHandler : IRequestHandler<UpdateSupplierProduct>
{
    private readonly ISupplierProductWriter _writer;
    public UpdateSupplierProductHandler(ISupplierProductWriter writer) => _writer = writer;
    public Task Handle(UpdateSupplierProduct cmd, CancellationToken ct)
        => _writer.Update(cmd.Dto, ct);
}

public class DeleteSupplierProductHandler : IRequestHandler<DeleteSupplierProduct>
{
    private readonly ISupplierProductWriter _writer;
    public DeleteSupplierProductHandler(ISupplierProductWriter writer) => _writer = writer;
    public Task Handle(DeleteSupplierProduct cmd, CancellationToken ct)
        => _writer.Delete(cmd.SupplierId, cmd.ProductId, ct);
}

public class GetSuppliersWithProductsHandler : IRequestHandler<GetSuppliersWithProducts, List<SupplierWithProductsDto>>
{
    private readonly ISupplierReader _supplierReader;
    private readonly ISupplierProductReader _supplierProductReader;
    private readonly IHttpClientFactory _httpClientFactory;

    public GetSuppliersWithProductsHandler(
        ISupplierReader supplierReader,
        ISupplierProductReader supplierProductReader,
        IHttpClientFactory httpClientFactory)
    {
        _supplierReader = supplierReader;
        _supplierProductReader = supplierProductReader;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<SupplierWithProductsDto>> Handle(GetSuppliersWithProducts request, CancellationToken ct)
    {
        // 1. Get all suppliers
        var suppliers = await _supplierReader.List(0, 1000, ct); // adjust as needed
        if (suppliers.Count == 0) return new();

        // 2. Get all supplier products
        var allSupplierIds = suppliers.Select(s => s.Id).ToList();
        var allSupplierProducts = new List<SupplierProductDto>();
        foreach (var supplierId in allSupplierIds)
        {
            var products = await _supplierProductReader.ListBySupplier(supplierId, ct);
            allSupplierProducts.AddRange(products);
        }
        if (allSupplierProducts.Count == 0) return suppliers.Select(s => new SupplierWithProductsDto(s.Id, s.Name, s.Contact, new())).ToList();

        // 3. Collect all productIds
        var allProductIds = allSupplierProducts.Select(sp => sp.ProductId).Distinct().ToList();

        // 4. Call Catalog API for product details
        var http = _httpClientFactory.CreateClient("Catalog");
        var catalogResponse = await http.PostAsJsonAsync("/v1/products/batch", allProductIds, ct);
        catalogResponse.EnsureSuccessStatusCode();
        var catalogProducts = await catalogResponse.Content.ReadFromJsonAsync<List<CatalogProductDto>>(cancellationToken: ct) ?? new();
        var productMap = catalogProducts.ToDictionary(p => p.Id);

        // 5. Compose result
        var result = new List<SupplierWithProductsDto>();
        foreach (var supplier in suppliers)
        {
            var suppliedProducts = allSupplierProducts.Where(sp => sp.SupplierId == supplier.Id).ToList();
            var suppliedProductDtos = new List<SuppliedProductDto>();
            foreach (var sp in suppliedProducts)
            {
                if (!productMap.TryGetValue(sp.ProductId, out var prod)) continue;
                suppliedProductDtos.Add(new SuppliedProductDto(
                    sp.ProductId,
                    sp.SupplierSku,
                    sp.Price,
                    sp.Currency,
                    sp.LeadTimeDays,
                    prod.Name,
                    prod.Sku,
                    prod.Description,
                    prod.ImageMainUrl
                ));
            }
            result.Add(new SupplierWithProductsDto(supplier.Id, supplier.Name, supplier.Contact, suppliedProductDtos));
        }
        return result;
    }

    private record CatalogProductDto(
        Guid Id,
        string Sku,
        string Name,
        string? Description,
        string? ImageMainUrl,
        bool IsActive,
        List<Guid> CategoryIds
    );
}
