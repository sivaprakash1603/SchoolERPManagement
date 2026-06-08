namespace SchoolERPManagementBLLibrary.Exceptions;

public sealed class DuplicateEntityException : BusinessRuleException
{
    public DuplicateEntityException(string entityName, string fieldName, string value)
        : base($"{entityName} with {fieldName} '{value}' already exists.")
    {
    }
}
