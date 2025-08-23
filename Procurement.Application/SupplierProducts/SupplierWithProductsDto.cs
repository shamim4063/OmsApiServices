namespace Procurement.Application.SupplierProducts;

public record SupplierWithProductsDto(
    Guid SupplierId,
    string Name,
    string? Contact,
    List<SuppliedProductDto> Products
);

public record SuppliedProductDto(
    Guid ProductId,
    string? SupplierSku,
    decimal Price,
    string Currency,
    int? LeadTimeDays,
    // From Catalog
    string ProductName,
    string ProductSku,
    string? Description,
    string? ImageMainUrl
);
