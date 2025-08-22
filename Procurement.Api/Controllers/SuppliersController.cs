using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace Procurement.Api.Controllers;

[ApiController]
[Route("v1/procurement/suppliers")]
public class SuppliersController : ControllerBase
{
    /// <summary>Get a single supplier by id.</summary>
    [HttpGet]
    public IActionResult GetById()
    {
        return Ok("Hello Procurement");
    }

}