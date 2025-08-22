using Procurement.Domain;

namespace Procurement.Application.Suppliers;

public interface ISupplierReader
{
    Task<SupplierDto?> ById(Guid id, CancellationToken ct);
    Task<IReadOnlyList<SupplierDto>> List(int skip, int take, CancellationToken ct);
}

public interface ISupplierWriter
{
    Task<Guid> Add(string name, string? contact, string? taxId, bool isActive, CancellationToken ct);
    Task Update(Guid id, string name, string? contact, string? taxId, bool isActive, CancellationToken ct);
    Task Delete(Guid supplierId, CancellationToken ct);
}
