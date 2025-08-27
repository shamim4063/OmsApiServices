using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections;

namespace BuildingBlocks.Web.Validation;

/// <summary>
/// MVC ActionFilter that runs FluentValidation on any action argument that has a registered IValidator&lt;T&gt;.
/// Throws ValidationException (your shared type) so the ProblemDetails middleware returns a 400.
/// </summary>
public sealed class ControllerValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var sp = context.HttpContext.RequestServices;

        foreach (var (name, arg) in context.ActionArguments)
        {
            if (arg is null) continue;

            // If it's a collection, validate each item if validator exists for the element type
            if (arg is IEnumerable enumerable && arg.GetType() != typeof(string))
            {
                var elementType = GetEnumerableElementType(arg.GetType());
                if (elementType is not null)
                {
                    ValidateEnumerable(enumerable, elementType, sp, context);
                    continue;
                }
            }

            // Validate a single DTO if IValidator<dtoType> exists
            ValidateObject(arg, arg.GetType(), sp, context);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { /* no-op */ }

    private static void ValidateEnumerable(IEnumerable items, Type elementType, IServiceProvider sp, ActionContext ctx)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(elementType);
        if (sp.GetService(validatorType) is not IValidator validator) return;

        var errors = new Dictionary<string, List<string>>();

        foreach (var item in items)
        {
            if (item is null) continue;
            var validationContextType = typeof(ValidationContext<>).MakeGenericType(elementType);
            var validationContext = Activator.CreateInstance(validationContextType, item)!;

            var result = (FluentValidation.Results.ValidationResult)
                validator.GetType()
                         .GetMethod("Validate", new[] { validationContextType })!
                         .Invoke(validator, new[] { validationContext })!;

            if (!result.IsValid)
            {
                foreach (var failure in result.Errors)
                {
                    var key = failure.PropertyName;
                    if (!errors.TryGetValue(key, out var list))
                        errors[key] = list = new List<string>();
                    list.Add(failure.ErrorMessage);
                }
            }
        }

        if (errors.Count > 0)
        {
            throw new Errors.ValidationException(errors.ToDictionary(k => k.Key, v => v.Value.ToArray()));
        }
    }

    private static void ValidateObject(object model, Type dtoType, IServiceProvider sp, ActionContext ctx)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(dtoType);
        if (sp.GetService(validatorType) is not IValidator validator) return;

        var validationContextType = typeof(ValidationContext<>).MakeGenericType(dtoType);
        var validationContext = Activator.CreateInstance(validationContextType, model)!;

        var result = (FluentValidation.Results.ValidationResult)
            validator.GetType()
                     .GetMethod("Validate", new[] { validationContextType })!
                     .Invoke(validator, new[] { validationContext })!;

        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            throw new Errors.ValidationException(errors);
        }
    }

    private static Type? GetEnumerableElementType(Type type)
    {
        if (type.IsArray) return type.GetElementType();
        var iface = type.GetInterfaces()
                        .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        return iface?.GetGenericArguments().FirstOrDefault();
    }
}
