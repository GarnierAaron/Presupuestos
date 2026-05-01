using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presupuestos.Application.Dto.Items;
using Presupuestos.Application.Services;

namespace Presupuestos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly ItemService _items;

    public ItemsController(ItemService items) => _items = items;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ItemDto>>> GetAll(CancellationToken ct) =>
        Ok(await _items.ListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ItemDto>> Get(Guid id, CancellationToken ct)
    {
        var dto = await _items.GetAsync(id, ct);
        return dto == null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ItemDto>> Create([FromBody] CreateItemDto dto, CancellationToken ct)
    {
        var created = await _items.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ItemDto>> Update(Guid id, [FromBody] UpdateItemDto dto, CancellationToken ct)
    {
        var updated = await _items.UpdateAsync(id, dto, ct);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await _items.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
