using Microsoft.EntityFrameworkCore;
using Procurement.Application.SupplierProducts;
using Procurement.Domain;

namespace Procurement.Infrastructure.Persistence.SupplierProducts;
public class SupplierProductReader : ISupplierProductReader
{
    private readonly ProcurementDbContext _db;
    public SupplierProductReader(ProcurementDbContext db) => _db = db;

    public async Task<SupplierProductDto?> ById(Guid supplierId, Guid productId, CancellationToken ct)
    {
        return await _db.SupplierProducts
            .Where(sp => sp.SupplierId == supplierId && sp.ProductId == productId)
            .Select(sp => new SupplierProductDto(
                sp.SupplierId,
                sp.ProductId,
                sp.SupplierSku,
                sp.Price,
                sp.Currency,
                sp.LeadTimeDays
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<SupplierProductDto>> ListBySupplier(Guid supplierId, CancellationToken ct)
    {
        return await _db.SupplierProducts
            .Where(sp => sp.SupplierId == supplierId)
            .Select(sp => new SupplierProductDto(
                sp.SupplierId,
                sp.ProductId,
                sp.SupplierSku,
                sp.Price,
                sp.Currency,
                sp.LeadTimeDays
            ))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SupplierProductDto>> List(int skip, int take, CancellationToken ct)
    {
        return await _db.SupplierProducts
            .OrderBy(sp => sp.SupplierId)
            .ThenBy(sp => sp.ProductId)
            .Skip(skip).Take(take)
            .Select(sp => new SupplierProductDto(
                sp.SupplierId,
                sp.ProductId,
                sp.SupplierSku,
                sp.Price,
                sp.Currency,
                sp.LeadTimeDays
            ))
            .ToListAsync(ct);
    }
}

