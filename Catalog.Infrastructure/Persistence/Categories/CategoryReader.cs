using Catalog.Application.Categories;
using Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence.Categories;

public sealed class CategoryReader : ICategoryReader
{
    private readonly CatalogDbContext _db;
    public CategoryReader(CatalogDbContext db) => _db = db;

    public Task<CategoryDto?> ById(Guid id, CancellationToken ct) =>
        _db.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description, c.ParentId))
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<CategoryDto>> List(int skip, int take, CancellationToken ct) =>
        await _db.Categories
            .OrderBy(c => c.Name)
            .Skip(skip).Take(take)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description, c.ParentId))
            .ToListAsync(ct);
}
