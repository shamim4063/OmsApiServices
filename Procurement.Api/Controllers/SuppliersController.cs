using Microsoft.AspNetCore.Mvc;
using MediatR;
using Procurement.Application.Suppliers;

namespace Procurement.Api.Controllers;

[ApiController]
[Route("v1/suppliers")]
public class SuppliersController : ControllerBase
{
    private readonly IMediator _mediator;
    public SuppliersController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get a single supplier by id.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var supplier = await _mediator.Send(new GetSupplierById(id), ct);
        return supplier is null ? NotFound() : Ok(supplier);
    }

    /// <summary>List suppliers (paginated).</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var suppliers = await _mediator.Send(new ListSuppliers(skip, take), ct);
        return Ok(suppliers);
    }

    /// <summary>Create a new supplier.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplier cmd, CancellationToken ct)
    {
        var id = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    /// <summary>Update an existing supplier.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplier cmd, CancellationToken ct)
    {
        if (id != cmd.Id) return BadRequest();
        await _mediator.Send(cmd, ct);
        return NoContent();
    }

    /// <summary>Delete a supplier.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteSupplier(id), ct);
        return NoContent();
    }
}