using DanceSchoolApi.Contracts;
using DanceSchoolApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanceSchoolApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TariffsController(TariffService tariffs) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TariffResponse>>> GetAll(CancellationToken cancellationToken, bool includeArchive = false) =>
        Ok(await tariffs.GetAllAsync(includeArchive, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TariffResponse>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await tariffs.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<TariffResponse>> Create(TariffRequest request, CancellationToken cancellationToken)
    {
        var tariff = await tariffs.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = tariff.TariffId }, tariff);
    }

    [HttpPost("{id:int}/replace")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<TariffResponse>> Replace(int id, TariffRequest request, CancellationToken cancellationToken)
    {
        var tariff = await tariffs.ReplaceAsync(id, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = tariff.TariffId }, tariff);
    }

    [HttpPost("{id:int}/archive")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Archive(int id, CancellationToken cancellationToken)
    {
        await tariffs.ArchiveAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/restore")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken)
    {
        await tariffs.RestoreAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await tariffs.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
