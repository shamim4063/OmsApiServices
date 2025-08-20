using Catalog.Domain;

namespace Catalog.Application.Products;

public interface IProductsWriter
{
    void Add(Product product);
    // Add other write ops as needed: Update, Remove, etc.
}
