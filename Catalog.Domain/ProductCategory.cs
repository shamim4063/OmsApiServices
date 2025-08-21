namespace Catalog.Domain;

public sealed class ProductCategory
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = default!;

    private ProductCategory() { }
    public ProductCategory(Guid productId, Guid categoryId)
    {
        ProductId = productId;
        CategoryId = categoryId;
    }
}
