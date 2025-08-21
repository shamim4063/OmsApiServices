using System.Collections.Generic;

namespace Catalog.Domain;

public sealed class Product
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Sku { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public string? ImageMainUrl { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation property for many-to-many with Category
    public ICollection<ProductCategory> ProductCategories { get; private set; } = new List<ProductCategory>();

    private Product() { }
    public Product(string sku, string name, string? description = null, string? imageUrl = null)
    { Sku = sku; Name = name; Description = description; ImageMainUrl = imageUrl; }
}

