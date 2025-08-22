using Microsoft.EntityFrameworkCore;
using Procurement.Application.Suppliers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Procurement.Infrastructure.Persistence.Suppliers
{
    public sealed class SupplierReader : ISupplierReader
    {
        private readonly ProcurementDbContext _db;
        public SupplierReader(ProcurementDbContext db) => _db = db;

        public Task<SupplierDto?> ById(Guid id, CancellationToken ct) =>
            _db.Suppliers
            .Where(c => c.Id == id)
            .Select(c => new SupplierDto(c.Id, c.Name, c.TaxId, c.Contact, c.IsActive, c.CreatedAt))
            .FirstOrDefaultAsync(ct);

        public async Task<IReadOnlyList<SupplierDto>> List(int skip, int take, CancellationToken ct) =>
            await _db.Suppliers
                .OrderBy(c => c.Name)
                .Skip(skip)
                .Take(take)
                .Select(c => new SupplierDto(c.Id, c.Name, c.TaxId, c.Contact, c.IsActive, c.CreatedAt))
                .ToListAsync(ct);
    }
}
