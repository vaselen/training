using System.ComponentModel.DataAnnotations;
using System.Data;
using DanceSchoolApi.Contracts;
using DanceSchoolApi.Data;
using DanceSchoolApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DanceSchoolApi.Services;

public class SubscriptionService(DanceSchoolContext context, TimeProvider timeProvider)
{
    private DateOnly Today => DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);

    public async Task<List<SubscriptionResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var today = Today;
        var subscriptions = await Details().OrderBy(s => s.SubscriptionId).ToListAsync(cancellationToken);
        return subscriptions.Select(s => ToResponse(s, s.Student, s.Team, s.Tariff, today)).ToList();
    }

    public async Task<SubscriptionResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var sub = await Details().SingleOrDefaultAsync(s => s.SubscriptionId == id, cancellationToken)
            ?? throw new EntityNotFoundException("Абонемент не найден.");
        return ToResponse(sub, sub.Student, sub.Team, sub.Tariff, Today);
    }

    public async Task<SubscriptionResponse> PurchaseAsync(PurchaseSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        Validator.ValidateObject(request, new ValidationContext(request), validateAllProperties: true);
        // Serializable предотвращает одновременную покупку дубликата и превышение вместимости.
        // При конфликте транзакций клиент получает 409 и может повторить запрос.
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var today = Today;
        var student = await context.Students.AsNoTracking().SingleOrDefaultAsync(s => s.StudentId == request.StudentId, cancellationToken)
            ?? throw new EntityNotFoundException("Ученик не найден.");
        var team = await context.Teams.AsNoTracking().SingleOrDefaultAsync(t => t.TeamId == request.TeamId, cancellationToken)
            ?? throw new EntityNotFoundException("Команда не найдена.");
        var tariff = await context.Tariffs.AsNoTracking().SingleOrDefaultAsync(t => t.TariffId == request.TariffId, cancellationToken)
            ?? throw new EntityNotFoundException("Тариф не найден.");

        if (tariff.IsArchive)
            throw new BusinessRuleException("Нельзя купить абонемент по архивному тарифу.");
        if (!team.IsAgeAllowed(student.GetAge(today)))
            throw new BusinessRuleException($"Ученик не подходит по возрасту (нужно {team.MinAge}-{team.MaxAge}).");

        var active = ActiveOn(context.Subscriptions, today);
        if (await active.AnyAsync(s => s.StudentId == request.StudentId && s.TeamId == request.TeamId, cancellationToken))
            throw new BusinessRuleException("У ученика уже есть действующий абонемент в эту команду.");

        var studentsCount = await active.Where(s => s.TeamId == request.TeamId)
            .Select(s => s.StudentId).Distinct().CountAsync(cancellationToken);
        if (studentsCount >= team.MaxStudents)
            throw new BusinessRuleException("В команде нет свободных мест.");

        if (tariff.DurationDays > DateOnly.MaxValue.DayNumber - today.DayNumber)
            throw new ValidationException("Продолжительность тарифа слишком велика для вычисления даты окончания.");

        var subscription = new Subscription(student.StudentId, team.TeamId, tariff.TariffId, today, tariff.DurationDays);
        context.Subscriptions.Add(subscription);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ToResponse(subscription, student, team, tariff, today);
    }

    public async Task<SubscriptionResponse> CancelAsync(int id, CancellationToken cancellationToken = default)
    {
        var today = Today;
        var subscription = await context.Subscriptions.Include(s => s.Student).Include(s => s.Team).Include(s => s.Tariff)
            .SingleOrDefaultAsync(s => s.SubscriptionId == id, cancellationToken)
            ?? throw new EntityNotFoundException("Абонемент не найден.");
        try
        {
            subscription.Cancel(today);
        }
        catch (InvalidOperationException exception)
        {
            throw new BusinessRuleException(exception.Message);
        }

        await context.SaveChangesAsync(cancellationToken);
        return ToResponse(subscription, subscription.Student, subscription.Team, subscription.Tariff, today);
    }

    public async Task<List<SubscriptionResponse>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var today = Today;
        var subscriptions = await ActiveOn(Details(), today).ToListAsync(cancellationToken);
        return subscriptions.Select(s => ToResponse(s, s.Student, s.Team, s.Tariff, today)).ToList();
    }

    public async Task<List<SubscriptionResponse>> GetExpiredAsync(CancellationToken cancellationToken = default)
    {
        var today = Today;
        var subscriptions = await Details().Where(s => !s.IsCancelled && s.EndDate <= today).ToListAsync(cancellationToken);
        return subscriptions.Select(s => ToResponse(s, s.Student, s.Team, s.Tariff, today)).ToList();
    }

    // Единое SQL-условие для сервисов; вычисляемое свойство IsActiveNow в SQL не переводится.
    internal static IQueryable<Subscription> ActiveOn(IQueryable<Subscription> subscriptions, DateOnly date) =>
        subscriptions.Where(s => !s.IsCancelled && s.PurchaseDate <= date && s.EndDate > date);

    private IQueryable<Subscription> Details() => context.Subscriptions.AsNoTracking()
        .Include(s => s.Student).Include(s => s.Team).Include(s => s.Tariff);

    private static SubscriptionResponse ToResponse(Subscription sub, Student student, Team team, Tariff tariff, DateOnly today) => new(
        sub.SubscriptionId, sub.StudentId, student.GetFullName(), sub.TeamId, team.Name,
        sub.TariffId, tariff.Price, sub.PurchaseDate, sub.EndDate,
        sub.PurchaseDate.AddDays(tariff.DurationDays), sub.IsCancelled, sub.IsActiveOn(today), sub.GetDaysLeft(today));
}
