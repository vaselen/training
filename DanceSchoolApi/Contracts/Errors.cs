namespace DanceSchoolApi.Contracts;

public class EntityNotFoundException(string message) : Exception(message);

public class BusinessRuleException(string message) : Exception(message);
