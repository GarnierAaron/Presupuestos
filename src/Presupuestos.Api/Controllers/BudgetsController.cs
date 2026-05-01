using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presupuestos.Application.Dto.Budgets;
using Presupuestos.Application.Services;

namespace Presupuestos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BudgetsController : ControllerBase
{
    private readonly BudgetAppService _budgets;

    public BudgetsController(BudgetAppService budgets) => _budgets = budgets;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BudgetDto>>> GetAll(CancellationToken ct) =>
        Ok(await _budgets.ListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BudgetDto>> Get(Guid id, CancellationToken ct)
    {
        var dto = await _budgets.GetAsync(id, ct);
        return dto == null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<BudgetDto>> Create([FromBody] CreateBudgetDto dto, CancellationToken ct)
    {
        try
        {
            var created = await _budgets.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await _budgets.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
