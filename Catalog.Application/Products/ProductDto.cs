
namespace Catalog.Application.Products;

public record ProductDto(string Sku, string Name, string? Description, string? ImageMainUrl, bool IsActive)
{
    private Guid id;

    public ProductDto(Guid id, string sku, string name, bool isActive)
        : this(sku, name, null, null, isActive) // Use 'this' constructor initializer to call the primary constructor
    {
        this.id = id;
    }
}
