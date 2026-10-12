using DanceSchoolApi.Contracts;
using DanceSchoolApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanceSchoolApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController(StudentService students) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<StudentResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await students.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StudentResponse>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await students.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<StudentResponse>> Create(StudentRequest request, CancellationToken cancellationToken)
    {
        var student = await students.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = student.StudentId }, student);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(int id, StudentRequest request, CancellationToken cancellationToken)
    {
        await students.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await students.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:int}/teams")]
    [Authorize]
    public async Task<ActionResult<List<StudentTeamResponse>>> GetStudentTeams(int id, CancellationToken cancellationToken) =>
        Ok(await students.GetTeamsAsync(id, cancellationToken));

    [HttpGet("eligible/{teamId:int}")]
    [Authorize]
    public async Task<IActionResult> GetEligibleForTeam(int teamId, CancellationToken cancellationToken)
    {
        var eligible = await students.GetEligibleForTeamAsync(teamId, cancellationToken);
        return Ok(new { message = $"Найдено учеников: {eligible.Count}", data = eligible });
    }
}