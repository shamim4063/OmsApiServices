using System.Collections.Generic;

namespace Catalog.Domain;

public sealed class Category
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public Guid? ParentId { get; private set; }

    // Navigation properties
    public Category? Parent { get; private set; }
    public ICollection<Category> Children { get; private set; } = new List<Category>();
    public ICollection<ProductCategory> ProductCategories { get; private set; } = new List<ProductCategory>();

    private Category() { }
    public Category(string name, string? description = null, Guid? parentId = null)
    {
        Name = name;
        Description = description;
        ParentId = parentId;
    }
}
