namespace Procurement.Application.Suppliers;

public record SupplierDto(
    Guid Id,
    string Name,
    string? Contact,
    string? TaxId,
    bool IsActive,
    DateTime CreatedAt
);
