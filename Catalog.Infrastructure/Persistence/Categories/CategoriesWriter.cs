using Catalog.Application.Categories;
using Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence.Categories;

public sealed class CategoriesWriter : ICategoriesWriter
{
    private readonly CatalogDbContext _db;
    public CategoriesWriter(CatalogDbContext db) => _db = db;

    public async Task<Guid> Add(string name, string? description, Guid? parentId, CancellationToken ct)
    {
        var category = new Category(name, description, parentId);
        _db.Categories.Add(category);
        await _db.SaveChangesAsync(ct);
        return category.Id;
    }

    public async Task Update(Guid id, string name, string? description, Guid? parentId, CancellationToken ct)
    {
        var category = await _db.Categories.FindAsync(new object[] { id }, ct);
        if (category == null) throw new ArgumentException("Category not found", nameof(id));
        category.GetType().GetProperty("Name")!.SetValue(category, name);
        category.GetType().GetProperty("Description")!.SetValue(category, description);
        category.GetType().GetProperty("ParentId")!.SetValue(category, parentId);
        await _db.SaveChangesAsync(ct);
    }

    public async Task Delete(Guid id, CancellationToken ct)
    {
        var category = await _db.Categories.FindAsync(new object[] { id }, ct);
        if (category == null) throw new ArgumentException("Category not found", nameof(id));
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(ct);
    }
}
