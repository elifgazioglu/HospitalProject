using FluentValidation;
using System.Reflection;

public class CommonValidation<TRequest> : AbstractValidator<TRequest>
{
    public CommonValidation()
    {
        // Get the type of TRequest
        var requestType = typeof(TRequest);

        // Find the 'Id' property
        var idProperty = requestType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                    .FirstOrDefault(p => p.Name == "Id");

        if (idProperty != null)
        {
            // Use reflection to validate the property
            RuleFor(r => r)
                .Must(r =>
                {
                    var idValue = idProperty.GetValue(r);
                    if (idValue == null) return false;

                    // Ensure the value is greater than 0
                    if (idValue is int intValue)
                    {
                        return intValue > 0;
                    }

                    // Handle other types if necessary
                    return false;
                })
                .WithMessage("Id must be greater than 0.");
        }
    }
}

public class IntValidator : AbstractValidator<int>
{
    public IntValidator()
    {
        RuleFor(x => x)
            .GreaterThan(0)
            .WithMessage("Value must be greater than 0.");
    }
}

