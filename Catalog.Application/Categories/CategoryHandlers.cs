using MediatR;

namespace Catalog.Application.Categories;

public class GetCategoryByIdHandler : IRequestHandler<GetCategoryById, CategoryDto?>
{
    private readonly ICategoryReader _reader;
    public GetCategoryByIdHandler(ICategoryReader reader) => _reader = reader;
    public Task<CategoryDto?> Handle(GetCategoryById request, CancellationToken ct)
        => _reader.ById(request.Id, ct);
}

public class ListCategoriesHandler : IRequestHandler<ListCategories, IReadOnlyList<CategoryDto>>
{
    private readonly ICategoryReader _reader;
    public ListCategoriesHandler(ICategoryReader reader) => _reader = reader;
    public Task<IReadOnlyList<CategoryDto>> Handle(ListCategories request, CancellationToken ct)
        => _reader.List(request.Skip, request.Take, ct);
}

public class CreateCategoryHandler : IRequestHandler<CreateCategory, Guid>
{
    private readonly ICategoriesWriter _writer;
    public CreateCategoryHandler(ICategoriesWriter writer) => _writer = writer;
    public Task<Guid> Handle(CreateCategory request, CancellationToken ct)
        => _writer.Add(request.Name, request.Description, request.ParentId, ct);
}

public class UpdateCategoryHandler : IRequestHandler<UpdateCategory>
{
    private readonly ICategoriesWriter _writer;
    public UpdateCategoryHandler(ICategoriesWriter writer) => _writer = writer;
    public async Task Handle(UpdateCategory request, CancellationToken ct)
        => await _writer.Update(request.Id, request.Name, request.Description, request.ParentId, ct);
}

public class DeleteCategoryHandler : IRequestHandler<DeleteCategory>
{
    private readonly ICategoriesWriter _writer;
    public DeleteCategoryHandler(ICategoriesWriter writer) => _writer = writer;
    public async Task Handle(DeleteCategory request, CancellationToken ct)
        => await _writer.Delete(request.Id, ct);
}
