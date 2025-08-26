using FluentValidation;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Procurement.Application.Suppliers;

public record SupplierDto(
    Guid Id,
    string Name,
    string? Contact,
    string? TaxId,
    bool IsActive,
    DateTime CreatedAt
);

public sealed record CreateSupplierCommand(
    string Name,
    string? Contact,
    string? TaxId,
    string? Email,
    bool IsActive);

public sealed class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    // Precompiled regexes
    private static readonly Regex NameRegex =
        new(@"^[\p{L}\p{N}\s\-\.'&]+$", RegexOptions.Compiled);

    private static readonly Regex PhoneRegex =
        new(@"^\+?[0-9\s\-\(\).]{7,20}$", RegexOptions.Compiled);

    private static readonly Regex TaxIdRegex =
        new(@"^[A-Z0-9\-]{5,20}$", RegexOptions.Compiled);

    public CreateSupplierCommandValidator()
    {
        // Name (required, trimmed)
        RuleFor(x => x.Name)
            .Must(s => !string.IsNullOrWhiteSpace(s?.Trim()))
                .WithMessage("Name is required.")
            .Must(s => s!.Trim().Length >= 2)
                .WithMessage("Name must be at least 2 characters.")
            .Must(s => s!.Trim().Length <= 100)
                .WithMessage("Name must be 100 characters or fewer.")
            .Must(s => NameRegex.IsMatch(s!.Trim()))
                .WithMessage("Name contains invalid characters.");

        // Email (optional) — validate only if provided; trim inside predicate
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .Must(s => string.IsNullOrWhiteSpace(s) || IsValidEmail(s!.Trim()))
                .WithMessage("Email is not a valid email address.")
            .Must(s => string.IsNullOrWhiteSpace(s) || s!.Trim().Length <= 254)
                .WithMessage("Email must be 254 characters or fewer.");

        // Contact (optional) — simple international phone format; trim inside predicate
        RuleFor(x => x.Contact)
            .Cascade(CascadeMode.Stop)
            .Must(s => string.IsNullOrWhiteSpace(s) || PhoneRegex.IsMatch(s!.Trim()))
                .WithMessage("Contact must be a valid phone number (7–20 digits).");

        // TaxId (optional) — trim + uppercase in predicate before matching
        RuleFor(x => x.TaxId)
            .Cascade(CascadeMode.Stop)
            .Must(s => string.IsNullOrWhiteSpace(s) || TaxIdRegex.IsMatch(s!.Trim().ToUpperInvariant()))
                .WithMessage("TaxId must be 5–20 characters (A–Z, 0–9, or -).");

        // Cross-field: require at least one way to reach them
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Email?.Trim()) ||
                       !string.IsNullOrWhiteSpace(x.Contact?.Trim()))
            .WithMessage("Provide at least one of Email or Contact.");
    }

    private static bool IsValidEmail(string input)
    {
        try
        {
            _ = new MailAddress(input);
            return true;
        }
        catch
        {
            return false;
        }
    }
}