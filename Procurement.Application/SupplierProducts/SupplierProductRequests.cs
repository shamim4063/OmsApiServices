using MediatR;

namespace Procurement.Application.SupplierProducts;

public sealed record GetSupplierProductById(Guid SupplierId, Guid ProductId) : IRequest<SupplierProductDto?>;
public sealed record ListSupplierProductsBySupplier(Guid SupplierId) : IRequest<IReadOnlyList<SupplierProductDto>>;
public sealed record ListSupplierProducts(int Skip, int Take) : IRequest<IReadOnlyList<SupplierProductDto>>;
public sealed record CreateSupplierProduct(SupplierProductDto Dto) : IRequest;
public sealed record UpdateSupplierProduct(SupplierProductDto Dto) : IRequest;
public sealed record DeleteSupplierProduct(Guid SupplierId, Guid ProductId) : IRequest;

public record GetSuppliersWithProducts : IRequest<List<SupplierWithProductsDto>>;
