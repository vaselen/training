using System.ComponentModel.DataAnnotations;
using System.Data;
using DanceSchoolApi.Contracts;
using DanceSchoolApi.Data;
using DanceSchoolApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DanceSchoolApi.Services;

public class TeamService(DanceSchoolContext context, TimeProvider timeProvider)
{
    private DateOnly Today => DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);

    public Task<List<TeamResponse>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Query(Today).OrderBy(t => t.TeamId).ToListAsync(cancellationToken);

    public async Task<TeamResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await Query(Today).SingleOrDefaultAsync(t => t.TeamId == id, cancellationToken)
            ?? throw new EntityNotFoundException("Команда не найдена.");

    public async Task<TeamResponse> CreateAsync(TeamRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(request, cancellationToken);
        var team = new Team();
        Apply(team, request);
        context.Teams.Add(team);
        await context.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(team.TeamId, cancellationToken);
    }

    public async Task UpdateAsync(int id, TeamRequest request, CancellationToken cancellationToken = default)
    {
        // Изменение вместимости и покупка должны согласовываться при одновременных запросах.
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await ValidateAsync(request, cancellationToken);
        var team = await context.Teams.SingleOrDefaultAsync(t => t.TeamId == id, cancellationToken)
            ?? throw new EntityNotFoundException("Команда не найдена.");
        var studentsCount = await SubscriptionService.ActiveOn(context.Subscriptions, Today)
            .Where(s => s.TeamId == id).Select(s => s.StudentId).Distinct().CountAsync(cancellationToken);
        if (request.MaxStudents < studentsCount)
            throw new BusinessRuleException("Вместимость команды не может быть меньше числа действующих участников.");

        Apply(team, request);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var team = await context.Teams.SingleOrDefaultAsync(t => t.TeamId == id, cancellationToken)
            ?? throw new EntityNotFoundException("Команда не найдена.");
        if (await context.Subscriptions.AnyAsync(s => s.TeamId == id, cancellationToken))
            throw new BusinessRuleException("Нельзя удалить команду с историей абонементов.");

        context.Teams.Remove(team);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<List<TeamResponse>> GetAvailableAsync(CancellationToken cancellationToken = default) =>
        Query(Today).Where(t => t.StudentsCount < t.MaxStudents).ToListAsync(cancellationToken);

    public async Task<List<StudentResponse>> GetStudentsAsync(int teamId, CancellationToken cancellationToken = default)
    {
        var today = Today;
        if (!await context.Teams.AnyAsync(t => t.TeamId == teamId, cancellationToken))
            throw new EntityNotFoundException("Команда не найдена.");

        var students = await SubscriptionService.ActiveOn(context.Subscriptions.AsNoTracking(), today)
            .Where(s => s.TeamId == teamId).Select(s => s.Student).Distinct().ToListAsync(cancellationToken);
        return students.Select(s => StudentService.ToResponse(s, today)).ToList();
    }

    private IQueryable<TeamResponse> Query(DateOnly today) => context.Teams.AsNoTracking().Select(team => new TeamResponse
    {
        TeamId = team.TeamId,
        Name = team.Name,
        TeacherId = team.TeacherId,
        TeacherName = (team.Teacher!.LastName + " " + team.Teacher.FirstName + " " + team.Teacher.MiddleName).Trim(),
        MinAge = team.MinAge,
        MaxAge = team.MaxAge,
        MaxStudents = team.MaxStudents,
        StudentsCount = team.Subscriptions.Where(s => !s.IsCancelled && s.PurchaseDate <= today && s.EndDate > today)
            .Select(s => s.StudentId).Distinct().Count()
    });

    private async Task ValidateAsync(TeamRequest request, CancellationToken cancellationToken)
    {
        Validator.ValidateObject(request, new ValidationContext(request), validateAllProperties: true);
        if (request.MaxAge < request.MinAge)
            throw new ValidationException("Максимальный возраст не может быть меньше минимального.");
        if (!await context.Teachers.AnyAsync(t => t.TeacherId == request.TeacherId, cancellationToken))
            throw new EntityNotFoundException("Учитель не найден.");
    }

    private static void Apply(Team team, TeamRequest request)
    {
        team.Name = request.Name.Trim();
        team.TeacherId = request.TeacherId;
        team.MinAge = request.MinAge;
        team.MaxAge = request.MaxAge;
        team.MaxStudents = request.MaxStudents;
    }
}
