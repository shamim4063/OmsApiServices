using Microsoft.AspNetCore.Mvc;
using MediatR;
using Catalog.Application.Products;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("v1/products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get a single product by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken ct)
    {
        var dto = await _mediator.Send(new GetProductById(id), ct);
        if (dto is null) return NotFound();
        return Ok(dto);
    }

    /// <summary>List products (paginated).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> List(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken ct = default)
    {
        if (take <= 0 || take > 100) take = 20;
        var list = await _mediator.Send(new ListProducts(skip, take), ct);
        return Ok(list);
    }

    /// <summary>Create a new product.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateProduct cmd, CancellationToken ct)
    {
        try
        {
            var id = await _mediator.Send(cmd, ct);
            return Created($"/v1/products/{id}", new { id });
        }
        catch (ArgumentException ex)
        {
            return ValidationProblem(title: "Invalid product data", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (InvalidOperationException ex) // e.g., SKU already exists
        {
            return Problem(title: "Product conflict", detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
        }
    }
}
