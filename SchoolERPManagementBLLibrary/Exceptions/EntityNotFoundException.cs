namespace SchoolERPManagementBLLibrary.Exceptions;

public sealed class EntityNotFoundException : BusinessRuleException
{
    public EntityNotFoundException(string entityName, string? identifier = null)
        : base(identifier is null
            ? $"{entityName} was not found."
            : $"{entityName} with identifier '{identifier}' was not found.")
    {
    }
}
