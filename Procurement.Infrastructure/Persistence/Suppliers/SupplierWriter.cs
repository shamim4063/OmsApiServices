using Procurement.Application.Suppliers;
using Procurement.Domain;
using System.Text.Json;

namespace Procurement.Infrastructure.Persistence.Suppliers
{
    public sealed class SupplierWriter : ISupplierWriter
    {
        private readonly ProcurementDbContext _db;
        public SupplierWriter(ProcurementDbContext db) => _db = db;
        public async Task<Guid> Add(string name, string? contact, string? taxId, bool isActive, CancellationToken ct)
        {
            var supplier = new Supplier(name, JsonSerializer.Serialize(contact), taxId, isActive);
            _db.Suppliers.Add(supplier);
            await _db.SaveChangesAsync(ct);
            return supplier.Id;
        }

        public async Task Delete(Guid supplierId, CancellationToken ct)
        {
            var supplier = await _db.Suppliers.FindAsync(new object?[] { supplierId }, ct);
            if (supplier is null) throw new ArgumentException("Supplier not found", nameof(supplierId));

            _db.Suppliers.Remove(supplier);
            await _db.SaveChangesAsync(ct);
        }

        public async Task Update(Guid id, string name, string? contact, string? taxId, bool isActive, CancellationToken ct)
        {
            var supplier = await _db.Suppliers.FindAsync(new object?[] { id }, ct);
            if (supplier is null) throw new ArgumentException("Supplier not found", nameof(id));

            supplier.GetType().GetProperty("Name")!.SetValue(supplier, name);
            supplier.GetType().GetProperty("Contact")!.SetValue(supplier, JsonSerializer.Serialize(contact));
            supplier.GetType().GetProperty("TaxId")!.SetValue(supplier, taxId);
            if (isActive) supplier.Activate(); else supplier.Deactivate();
            await _db.SaveChangesAsync(ct);
        }
    }
}
