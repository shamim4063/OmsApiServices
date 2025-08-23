using Procurement.Application.SupplierProducts;
using Procurement.Domain;

namespace Procurement.Infrastructure.Persistence.SupplierProducts;

public class SupplierProductWriter : ISupplierProductWriter
{
    private readonly ProcurementDbContext _db;
    public SupplierProductWriter(ProcurementDbContext db) => _db = db;

    public async Task Add(SupplierProductDto dto, CancellationToken ct)
    {
        var entity = new SupplierProduct(
            dto.SupplierId,
            dto.ProductId,
            dto.SupplierSku,
            dto.Price,
            dto.Currency,
            dto.LeadTimeDays
        );
        _db.SupplierProducts.Add(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task Update(SupplierProductDto dto, CancellationToken ct)
    {
        var entity = await _db.SupplierProducts.FindAsync(new object?[] { dto.SupplierId, dto.ProductId }, ct);
        if (entity == null) throw new InvalidOperationException("SupplierProduct not found");
        // Update properties
        entity.GetType().GetProperty("SupplierSku")?.SetValue(entity, dto.SupplierSku);
        entity.GetType().GetProperty("Price")?.SetValue(entity, dto.Price);
        entity.GetType().GetProperty("Currency")?.SetValue(entity, dto.Currency);
        entity.GetType().GetProperty("LeadTimeDays")?.SetValue(entity, dto.LeadTimeDays);
        await _db.SaveChangesAsync(ct);
    }

    public async Task Delete(Guid supplierId, Guid productId, CancellationToken ct)
    {
        var entity = await _db.SupplierProducts.FindAsync(new object?[] { supplierId, productId }, ct);
        if (entity != null)
        {
            _db.SupplierProducts.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }
}
