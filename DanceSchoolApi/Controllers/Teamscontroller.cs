using DanceSchoolApi.Contracts;
using DanceSchoolApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanceSchoolApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsController(TeamService teams, SubscriptionService subscriptions) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TeamResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await teams.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TeamResponse>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await teams.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<TeamResponse>> Create(TeamRequest request, CancellationToken cancellationToken)
    {
        var team = await teams.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = team.TeamId }, team);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(int id, TeamRequest request, CancellationToken cancellationToken)
    {
        await teams.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await teams.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:int}/students")]
    [Authorize]
    public async Task<ActionResult<List<StudentResponse>>> GetTeamStudents(int id, CancellationToken cancellationToken) =>
        Ok(await teams.GetStudentsAsync(id, cancellationToken));

    [HttpGet("available")]
    [Authorize]
    public async Task<ActionResult<List<TeamResponse>>> GetAvailableTeams(CancellationToken cancellationToken) =>
        Ok(await teams.GetAvailableAsync(cancellationToken));

    [HttpPost("enroll")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<SubscriptionResponse>> Enroll(int teamId, int studentId, int tariffId, CancellationToken cancellationToken)
    {
        var subscription = await subscriptions.PurchaseAsync(new PurchaseSubscriptionRequest
        {
            TeamId = teamId, StudentId = studentId, TariffId = tariffId
        }, cancellationToken);
        return CreatedAtAction(nameof(SubscriptionsController.GetById), "Subscriptions",
            new { id = subscription.SubscriptionId }, subscription);
    }
}