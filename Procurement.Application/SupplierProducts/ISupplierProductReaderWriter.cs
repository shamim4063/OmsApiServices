using Procurement.Domain;

namespace Procurement.Application.SupplierProducts;

public interface ISupplierProductReader
{
    Task<SupplierProductDto?> ById(Guid supplierId, Guid productId, CancellationToken ct);
    Task<IReadOnlyList<SupplierProductDto>> ListBySupplier(Guid supplierId, CancellationToken ct);
    Task<IReadOnlyList<SupplierProductDto>> List(int skip, int take, CancellationToken ct);
}

public interface ISupplierProductWriter
{
    Task Add(SupplierProductDto dto, CancellationToken ct);
    Task Update(SupplierProductDto dto, CancellationToken ct);
    Task Delete(Guid supplierId, Guid productId, CancellationToken ct);
}
