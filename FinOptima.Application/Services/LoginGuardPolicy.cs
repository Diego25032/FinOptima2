namespace FinOptima.Application.Services;

/// <summary>
/// Regla de bloqueo por intentos fallidos. El contador se guarda en el servidor
/// (columnas de la tabla profiles) para que un intento en un equipo tambien bloquee el otro.
/// La base de datos recibe estos valores como argumentos de la funcion RPC, de modo que
/// la unica definicion de la regla vive aqui.
/// </summary>
public static class LoginGuardPolicy
{
    public const int MaximumAttempts = 5;
    public const int LockMinutes = 15;

    public static bool ShouldLock(int failedAttempts) => failedAttempts >= MaximumAttempts;

    public static DateTime? LockUntil(int failedAttempts, DateTime now) =>
        ShouldLock(failedAttempts) ? now.AddMinutes(LockMinutes) : null;

    /// <summary>Texto que ve quien intenta entrar, segun los intentos acumulados.</summary>
    public static string MessageFor(int failedAttempts, DateTime now)
    {
        if (failedAttempts >= MaximumAttempts)
            return $"Demasiados intentos fallidos. La cuenta queda bloqueada hasta las {now.AddMinutes(LockMinutes):HH:mm}.";
        return $"Correo o contraseña incorrectos. Intentos: {failedAttempts}/{MaximumAttempts}.";
    }
}
