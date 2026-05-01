using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presupuestos.Application.Dto.Services;
using Presupuestos.Application.Services;

namespace Presupuestos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly ServiceCatalogService _services;

    public ServicesController(ServiceCatalogService services) => _services = services;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServiceDto>>> GetAll(CancellationToken ct) =>
        Ok(await _services.ListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceDto>> Get(Guid id, CancellationToken ct)
    {
        var dto = await _services.GetAsync(id, ct);
        return dto == null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceDto>> Create([FromBody] CreateServiceDto dto, CancellationToken ct)
    {
        try
        {
            var created = await _services.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ServiceDto>> Update(Guid id, [FromBody] UpdateServiceDto dto, CancellationToken ct)
    {
        try
        {
            var updated = await _services.UpdateAsync(id, dto, ct);
            return updated == null ? NotFound() : Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await _services.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
