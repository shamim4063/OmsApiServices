using System;
using System.Collections.Generic;

namespace Procurement.Domain;

public sealed class Supplier
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Contact { get; private set; } // JSONB in DB, string in domain
    public string? TaxId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Supplier() { }

    public Supplier(string name, string? contact = null, string? taxId = null, bool isActive = true)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Contact = contact;
        TaxId = taxId;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
