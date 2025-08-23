using MediatR;
using Microsoft.AspNetCore.Mvc;
using Procurement.Application.SupplierProducts;

namespace Procurement.Api.Controllers;

[ApiController]
[Route("v1/supplier-products")]
public class SupplierProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    public SupplierProductsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{supplierId:guid}/{productId:guid}")]
    public async Task<ActionResult<SupplierProductDto>> GetById(Guid supplierId, Guid productId, CancellationToken ct)
    {
        var dto = await _mediator.Send(new GetSupplierProductById(supplierId, productId), ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpGet("by-supplier/{supplierId:guid}")]
    public async Task<ActionResult<IReadOnlyList<SupplierProductDto>>> ListBySupplier(Guid supplierId, CancellationToken ct)
        => Ok(await _mediator.Send(new ListSupplierProductsBySupplier(supplierId), ct));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SupplierProductDto>>> List([FromQuery] int skip = 0, [FromQuery] int take = 20, CancellationToken ct = default)
        => Ok(await _mediator.Send(new ListSupplierProducts(skip, take), ct));

    [HttpGet("suppliers-with-products")]
    public async Task<ActionResult<List<SupplierWithProductsDto>>> Get(CancellationToken ct)
        => Ok(await _mediator.Send(new GetSuppliersWithProducts(), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SupplierProductDto dto, CancellationToken ct)
    {
        await _mediator.Send(new CreateSupplierProduct(dto), ct);
        return CreatedAtAction(nameof(GetById), new { supplierId = dto.SupplierId, productId = dto.ProductId }, null);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] SupplierProductDto dto, CancellationToken ct)
    {
        await _mediator.Send(new UpdateSupplierProduct(dto), ct);
        return NoContent();
    }

    [HttpDelete("{supplierId:guid}/{productId:guid}")]
    public async Task<IActionResult> Delete(Guid supplierId, Guid productId, CancellationToken ct)
    {
        await _mediator.Send(new DeleteSupplierProduct(supplierId, productId), ct);
        return NoContent();
    }
}
