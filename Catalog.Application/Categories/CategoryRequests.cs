using MediatR;

namespace Catalog.Application.Categories;

public sealed record GetCategoryById(Guid Id) : IRequest<CategoryDto?>;
public sealed record ListCategories(int Skip, int Take) : IRequest<IReadOnlyList<CategoryDto>>;
public sealed record CreateCategory(string Name, string? Description, Guid? ParentId) : IRequest<Guid>;
public sealed record UpdateCategory(Guid Id, string Name, string? Description, Guid? ParentId) : IRequest;
public sealed record DeleteCategory(Guid Id) : IRequest;
