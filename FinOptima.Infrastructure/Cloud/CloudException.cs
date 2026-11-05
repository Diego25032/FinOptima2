namespace FinOptima.Infrastructure.Cloud;

public enum CloudFailure
{
    None,
    /// <summary>Correo o contraseña incorrectos.</summary>
    Credentials,
    /// <summary>La cuenta existe pero todavia no confirma el correo.</summary>
    NotConfirmed,
    /// <summary>Supabase esta limitando la cantidad de intentos.</summary>
    RateLimited,
    /// <summary>El equipo no tiene conexion o la URL del proyecto esta mal.</summary>
    Network,
    /// <summary>RLS o permisos: la sesion perdio validez o no se puede tocar esa fila.</summary>
    Permission,
    /// <summary>Dato duplicado (por ejemplo un correo ya registrado).</summary>
    Conflict,
    /// <summary>La cuenta esta desactivada por administracion.</summary>
    Inactive,
    Other
}

/// <summary>Error de la nube ya traducido a un mensaje que puede mostrar el usuario.</summary>
public sealed class CloudException : Exception
{
    public CloudFailure Failure { get; }
    public string? ServerCode { get; }

    public CloudException(CloudFailure failure, string message, string? serverCode = null) : base(message)
    {
        Failure = failure;
        ServerCode = serverCode;
    }

    /// <summary>Mensaje tecnico para bitacoras y ventanillas de diagnostico.</summary>
    public string TechnicalMessage { get; init; } = "";
}
