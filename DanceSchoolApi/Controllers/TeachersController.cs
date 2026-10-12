using DanceSchoolApi.Contracts;
using DanceSchoolApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanceSchoolApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeachersController(TeacherService teachers) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TeacherResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await teachers.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TeacherResponse>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await teachers.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<TeacherResponse>> Create(TeacherRequest request, CancellationToken cancellationToken)
    {
        var teacher = await teachers.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = teacher.TeacherId }, teacher);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(int id, TeacherRequest request, CancellationToken cancellationToken)
    {
        await teachers.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await teachers.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
