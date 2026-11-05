namespace FinOptima.Application.Validators;

/// <summary>
/// Politica de contraseñas de FinOptima. La validacion vive aqui porque las ventanillas
/// de registro y de cambio de contraseña deben exigir el mismo criterio.
/// </summary>
public static class PasswordPolicy
{
    public const int MinimumLength = 8;

    public const string Requirement = "La contraseña debe tener al menos 8 caracteres e incluir por lo menos un número.";

    public static bool IsStrongEnough(string? password) => MeasuresMinimumLength(password) && ContainsDigit(password);

    static bool MeasuresMinimumLength(string? password) => !string.IsNullOrWhiteSpace(password) && password.Length >= MinimumLength;

    static bool ContainsDigit(string? password) => !string.IsNullOrWhiteSpace(password) && password.Any(char.IsDigit);
}
