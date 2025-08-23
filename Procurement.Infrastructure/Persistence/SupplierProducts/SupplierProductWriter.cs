using System;
using System.Threading;
using System.Threading.Tasks;
using Procurement.Domain;

namespace Procurement.Infrastructure.Persistence.SupplierProducts
{
    public interface ISupplierProductWriter
    {
        Task AddAsync(SupplierProduct entity, CancellationToken ct = default);
        Task UpdateAsync(SupplierProduct entity, CancellationToken ct = default);
        Task DeleteAsync(Guid supplierId, Guid productId, CancellationToken ct = default);
    }

    public class SupplierProductWriter : ISupplierProductWriter
    {
        private readonly ProcurementDbContext _db;
        public SupplierProductWriter(ProcurementDbContext db) => _db = db;

        public async Task AddAsync(SupplierProduct entity, CancellationToken ct = default)
        {
            await _db.SupplierProducts.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(SupplierProduct entity, CancellationToken ct = default)
        {
            _db.SupplierProducts.Update(entity);
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid supplierId, Guid productId, CancellationToken ct = default)
        {
            var entity = await _db.SupplierProducts.FindAsync(new object[] { supplierId, productId }, ct);
            if (entity != null)
            {
                _db.SupplierProducts.Remove(entity);
                await _db.SaveChangesAsync(ct);
            }
        }
    }
}
