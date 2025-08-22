using MediatR;

namespace Procurement.Application.Suppliers;

public sealed record GetSupplierById(Guid Id) : IRequest<SupplierDto?>;
public sealed record ListSuppliers(int Skip, int Take) : IRequest<IReadOnlyList<SupplierDto>>;
public sealed record CreateSupplier(
    string Name,
    string? Contact,
    string? TaxId,
    bool IsActive
) : IRequest<Guid>;
public sealed record UpdateSupplier(
    Guid Id,
    string Name,
    string? Contact,
    string? TaxId,
    bool IsActive
) : IRequest;
public sealed record DeleteSupplier(Guid Id) : IRequest;
