using System;

namespace Procurement.Domain
{
    public sealed class SupplierProduct
    {
        public Guid SupplierId { get; private set; }
        public Guid ProductId { get; private set; }
        public string? SupplierSku { get; private set; }
        public decimal Price { get; private set; }
        public string Currency { get; private set; }
        public int? LeadTimeDays { get; private set; }

        // Navigation property (optional, for EF Core)
        public Supplier? Supplier { get; private set; }

        private SupplierProduct() { }

        public SupplierProduct(Guid supplierId, Guid productId, string? supplierSku, decimal price, string currency, int? leadTimeDays = null)
        {
            SupplierId = supplierId;
            ProductId = productId;
            SupplierSku = supplierSku;
            Price = price;
            Currency = currency ?? throw new ArgumentNullException(nameof(currency));
            LeadTimeDays = leadTimeDays;
        }
    }
}