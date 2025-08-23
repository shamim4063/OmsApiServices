using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Procurement.Domain;

namespace Procurement.Infrastructure.Persistence.SupplierProducts
{
    public interface ISupplierProductReader
    {
        Task<SupplierProduct?> GetByIdAsync(Guid supplierId, Guid productId, CancellationToken ct = default);
        Task<List<SupplierProduct>> ListBySupplierAsync(Guid supplierId, CancellationToken ct = default);
    }

    public class SupplierProductReader : ISupplierProductReader
    {
        private readonly ProcurementDbContext _db;
        public SupplierProductReader(ProcurementDbContext db) => _db = db;

        public async Task<SupplierProduct?> GetByIdAsync(Guid supplierId, Guid productId, CancellationToken ct = default)
        {
            return await _db.SupplierProducts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SupplierId == supplierId && x.ProductId == productId, ct);
        }

        public async Task<List<SupplierProduct>> ListBySupplierAsync(Guid supplierId, CancellationToken ct = default)
        {
            return await _db.SupplierProducts
                .AsNoTracking()
                .Where(x => x.SupplierId == supplierId)
                .ToListAsync(ct);
        }
    }
}
