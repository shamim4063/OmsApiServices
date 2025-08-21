using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Catalog.Application.Categories;

public interface ICategoryReader
{
    Task<CategoryDto?> ById(Guid id, CancellationToken ct);
    Task<IReadOnlyList<CategoryDto>> List(int skip, int take, CancellationToken ct);
}

public interface ICategoriesWriter
{
    Task<Guid> Add(string name, string? description, Guid? parentId, CancellationToken ct);
    Task Update(Guid id, string name, string? description, Guid? parentId, CancellationToken ct);
    Task Delete(Guid id, CancellationToken ct);
}
