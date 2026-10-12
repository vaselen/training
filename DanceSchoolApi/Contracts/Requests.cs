using System.ComponentModel.DataAnnotations;

namespace DanceSchoolApi.Contracts;

public class StudentRequest
{
    [Required]
    public string FirstName { get; init; } = string.Empty;
    [Required]
    public string LastName { get; init; } = string.Empty;
    public string? MiddleName { get; init; }
    public DateOnly DateOfBirth { get; init; }
    [MaxLength(20)]
    public string Phone { get; init; } = string.Empty;
}

public class TeacherRequest
{
    [Required]
    public string FirstName { get; init; } = string.Empty;
    [Required]
    public string LastName { get; init; } = string.Empty;
    public string? MiddleName { get; init; }
    public DateOnly DateOfBirth { get; init; }
    public string Style { get; init; } = string.Empty;
    [Range(typeof(decimal), "0", "9.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public decimal Rating { get; init; }
}

public class TeamRequest
{
    [Required]
    public string Name { get; init; } = string.Empty;
    [Range(1, int.MaxValue)]
    public int TeacherId { get; init; }
    [Range(0, int.MaxValue)]
    public int MinAge { get; init; }
    [Range(0, int.MaxValue)]
    public int MaxAge { get; init; }
    [Range(1, int.MaxValue)]
    public int MaxStudents { get; init; }
}

public class TariffRequest
{
    [Range(1, int.MaxValue)]
    public int DurationDays { get; init; }
    [Range(typeof(decimal), "0", "99999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public decimal Price { get; init; }
}

public class PurchaseSubscriptionRequest
{
    [Range(1, int.MaxValue)]
    public int StudentId { get; init; }
    [Range(1, int.MaxValue)]
    public int TeamId { get; init; }
    [Range(1, int.MaxValue)]
    public int TariffId { get; init; }
}
