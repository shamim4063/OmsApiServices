namespace Catalog.Application.Categories;

public record CategoryDto(Guid Id, string Name, string? Description, Guid? ParentId);
