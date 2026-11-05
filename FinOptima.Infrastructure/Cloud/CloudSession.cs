namespace FinOptima.Infrastructure.Cloud;

/// <summary>
/// Sesion de la persona conectada. Vive solo en memoria durante la ejecucion:
/// los tokens no se escriben en disco, asi otro usuario del mismo computador no
/// puede reutilizarlos y basta con cerrar la aplicacion para cerrar sesion.
/// </summary>
public sealed class CloudSession
{
    readonly SupabaseGateway gateway;
    readonly SemaphoreSlim refreshLock = new(1, 1);
    AuthTokens? tokens;

    public CloudSession(SupabaseGateway gateway)
    {
        this.gateway = gateway;
    }

    public int ProfileId { get; private set; }
    public string Role { get; private set; } = "";
    public string Email { get; private set; } = "";
    public bool HasSession => tokens is { HasSession: true };

    /// <summary>Se dispara cuando el servidor rechazo la sesion y hay que volver al login.</summary>
    public event Action? SessionLost;

    /// <summary>
    /// Guarda los tokens de la persona que acaba de autenticarse. El perfil se carga
    /// despues con una consulta autenticada y se aplica con UpdateProfile.
    /// </summary>
    public void Start(AuthTokens auth)
    {
        tokens = auth;
        ProfileId = 0;
    }

    public void UpdateProfile(ProfileRow profile)
    {
        ProfileId = profile.Id;
        Role = profile.Role;
        Email = profile.Email;
    }

    public void Clear()
    {
        tokens = null;
        ProfileId = 0;
        Role = "";
        Email = "";
    }

    public async Task SignOutAsync()
    {
        if (tokens is { HasSession: true })
        {
            try
            {
                await gateway.SignOutAsync(tokens.AccessToken).ConfigureAwait(false);
            }
            catch (CloudException)
            {
                // Sin red o con el token ya caducado tambien hay que cerrar el lado local.
            }
        }
        Clear();
    }

    /// <summary>Token valido, renovando cuando falte menos de un minuto para que expire.</summary>
    public async Task<string> AccessTokenAsync()
    {
        if (tokens is null || !tokens.HasSession)
            throw new CloudException(CloudFailure.Permission, "No hay sesion activa en FinOptima. Inicia sesion de nuevo.");
        if (tokens.ExpiresAt > DateTime.UtcNow.AddMinutes(1)) return tokens.AccessToken;

        await refreshLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (tokens is not null && tokens.ExpiresAt > DateTime.UtcNow.AddMinutes(1)) return tokens.AccessToken;
            var refreshed = await gateway.RefreshAsync(tokens!.RefreshToken).ConfigureAwait(false);
            refreshed = refreshed with { AuthUserId = tokens.AuthUserId, Email = string.IsNullOrWhiteSpace(refreshed.Email) ? tokens.Email : refreshed.Email };
            tokens = refreshed;
            return refreshed.AccessToken;
        }
        catch (CloudException ex) when (ex.Failure is CloudFailure.Permission or CloudFailure.Credentials)
        {
            Clear();
            SessionLost?.Invoke();
            throw new CloudException(CloudFailure.Permission, "La sesion expiro. Inicia sesion otra vez para continuar.");
        }
        finally
        {
            refreshLock.Release();
        }
    }

    /// <summary>Ejecuta una operacion autenticada y espera el resultado desde la interfaz de Windows Forms.</summary>
    public T Query<T>(Func<string, Task<T>> operation)
    {
        try
        {
            return Sync(async () =>
            {
                var token = await AccessTokenAsync().ConfigureAwait(false);
                return await operation(token).ConfigureAwait(false);
            });
        }
        catch (CloudException ex) when (ex.Failure == CloudFailure.Permission)
        {
            Clear();
            SessionLost?.Invoke();
            throw;
        }
    }

    public void Execute(Func<string, Task> operation)
    {
        Query<object?>(async token =>
        {
            await operation(token).ConfigureAwait(false);
            return null;
        });
    }

    /// <summary>Llamadas publicas que no necesitan sesion: guardia de intentos y registro.</summary>
    public static T Anonymous<T>(Func<Task<T>> operation) => Sync(operation);

    /// <summary>Igual que Anonymous pero para llamadas que no devuelven nada.</summary>
    public static void FireAndWait(Func<Task> operation) => Sync(async () =>
    {
        await operation().ConfigureAwait(false);
        return 0;
    });

    /// <summary>
    /// Puente sincrono entre las ventanas de Windows Forms y las llamadas HTTP.
    /// No enquista la interfaz porque todas las esperas internas usan ConfigureAwait(false),
    /// asi ninguna continuacion necesita volver al hilo de la ventana.
    /// </summary>
    static T Sync<T>(Func<Task<T>> operation)
    {
        try { return operation().ConfigureAwait(false).GetAwaiter().GetResult(); }
        catch (AggregateException aggregate) when (aggregate.InnerException is not null) { throw aggregate.InnerException; }
    }
}
