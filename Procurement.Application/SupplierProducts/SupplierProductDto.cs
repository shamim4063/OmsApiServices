namespace Procurement.Application.SupplierProducts;

public record SupplierProductDto(
    Guid SupplierId,
    Guid ProductId,
    string? SupplierSku,
    decimal Price,
    string Currency,
    int? LeadTimeDays
);
