using FinOptima.Application.Interfaces;
using FinOptima.Application.Services;
using FinOptima.Application.Validators;
using FinOptima.Domain.Entities;

namespace FinOptima.Infrastructure.Cloud;

/// <summary>
/// Autenticacion contra Supabase Auth. Las contraseñas se verifican en el servidor,
/// FinOptima ya no guarda ningun hash. El bloqueo por intentos vive en la base de datos
/// para que un intento fallido en un computador tambien bloquee el otro.
/// </summary>
public sealed class SupabaseAuthenticationService(CloudSession session, SupabaseGateway gateway) : IAuthenticationService
{
    public bool IsConfigured => gateway.Settings.IsConfigured;
    public string ProjectUrl => gateway.Settings.ProjectUrl;
    public bool HasSession => session.HasSession;

    public User SignIn(string email, string password)
    {
        email = (email ?? "").Trim();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
            throw new CloudException(CloudFailure.Credentials, "Escribe el correo y la contraseña de tu cuenta.");

        var guard = GetGuard(email);
        if (guard.IsLocked)
            throw new CloudException(CloudFailure.RateLimited,
                $"La cuenta quedo bloqueada temporalmente. Puedes intentar de nuevo despues de las {guard.LockedUntil!.Value:HH:mm}.");

        AuthTokens tokens;
        try
        {
            tokens = CloudSession.Anonymous(() => gateway.SignInAsync(email, password));
        }
        catch (CloudException ex) when (ex.Failure == CloudFailure.Credentials)
        {
            var attempts = RecordFailure(email);
            throw new CloudException(CloudFailure.Credentials, LoginGuardPolicy.MessageFor(attempts, DateTime.Now));
        }

        // Los tokens entran primero en la sesion: la consulta del perfil se hace con esa sesion,
        // porque Row Level Security solo deja ver el perfil de quien trajo el token.
        session.Start(tokens);
        ProfileRow profile;
        try
        {
            profile = LoadProfile(tokens.AuthUserId);
        }
        catch
        {
            session.Clear();
            throw;
        }

        if (!profile.Active)
        {
            session.Clear();
            CloudSession.FireAndWait(() => gateway.SignOutAsync(tokens.AccessToken));
            throw new CloudException(CloudFailure.Inactive, "Esta cuenta esta desactivada. Contacta al administrador de FinOptima.");
        }

        session.UpdateProfile(profile);
        ClearGuard(email);
        return profile.ToEntity();
    }

    public SignUpResult SignUp(string email, string password, string fullName)
    {
        email = (email ?? "").Trim();
        fullName = string.IsNullOrWhiteSpace(fullName) ? email.Split('@')[0] : fullName.Trim();
        if (!email.Contains('@'))
            throw new CloudException(CloudFailure.Credentials, "El correo debe tener el formato nombre@dominio.com.");
        if (!PasswordPolicy.IsStrongEnough(password))
            throw new CloudException(CloudFailure.Credentials, PasswordPolicy.Requirement);

        var tokens = CloudSession.Anonymous(() => gateway.SignUpAsync(email, password, fullName));
        if (tokens.RequiresConfirmation)
            return new SignUpResult(false, null,
                "Casi listo: Supabase envio un correo de confirmacion. Abre ese enlace y luego inicia sesion.");

        session.Start(tokens);
        var profile = LoadProfile(tokens.AuthUserId);
        session.UpdateProfile(profile);
        ClearGuard(email);
        return new SignUpResult(true, profile.ToEntity(), "Cuenta de usuario creada. Ya puedes iniciar sesion.");
    }

    public void SignOut() => CloudSession.FireAndWait(() => session.SignOutAsync());

    /// <summary>
    /// Cambio de contraseña en tres pasos: Supabase exige sesion reciente, y al cambiar la
    /// contraseña revoca las demas sesiones. Por eso primero se vuelve a entrar con la actual,
    /// luego se guarda la nueva y por ultimo se abre sesion con ella para no perder el trabajo.
    /// </summary>
    public void ChangePassword(string currentPassword, string newPassword)
    {
        if (!session.HasSession)
            throw new CloudException(CloudFailure.Permission, "No hay sesion activa para cambiar la contraseña.");
        if (!PasswordPolicy.IsStrongEnough(newPassword))
            throw new CloudException(CloudFailure.Credentials, PasswordPolicy.Requirement);

        var email = session.Email;
        AuthTokens freshSession;
        try
        {
            freshSession = CloudSession.Anonymous(() => gateway.SignInAsync(email, currentPassword ?? ""));
        }
        catch (CloudException ex) when (ex.Failure == CloudFailure.Credentials)
        {
            throw new CloudException(CloudFailure.Credentials, "La contraseña actual no es correcta.");
        }

        session.Start(freshSession);
        session.UpdateProfile(LoadProfile(freshSession.AuthUserId));
        try
        {
            CloudSession.FireAndWait(() => gateway.UpdatePasswordAsync(freshSession.AccessToken, newPassword));
        }
        catch (CloudException)
        {
            // La contraseña sigue siendo la actual: se reabre la sesion anterior para no dejar al usuario fuera.
            var restored = CloudSession.Anonymous(() => gateway.SignInAsync(email, currentPassword));
            session.Start(restored);
            session.UpdateProfile(LoadProfile(restored.AuthUserId));
            throw;
        }

        var withNewPassword = CloudSession.Anonymous(() => gateway.SignInAsync(email, newPassword));
        session.Start(withNewPassword);
        session.UpdateProfile(LoadProfile(withNewPassword.AuthUserId));
    }

    public LoginGuard GetGuard(string email)
    {
        try
        {
            var rows = CloudSession.Anonymous(() => gateway.RpcAsync<GuardRow>("login_guard_state",
                new { p_email = (email ?? "").Trim().ToLowerInvariant() }, accessToken: null));
            var row = rows.FirstOrDefault();
            return new LoginGuard(row?.Attempts ?? 0, row?.LockedUntil);
        }
        catch (CloudException ex) when (ex.Failure == CloudFailure.Other)
        {
            // Si todavia no se ejecuto la migracion de guardia, el login sigue funcionando sin bloqueo.
            return new LoginGuard(0, null);
        }
    }

    public void ClearGuard(string email) => TryGuard(() => gateway.RpcAsync("login_guard_reset",
        new { p_email = email.Trim().ToLowerInvariant() }, accessToken: null));

    int RecordFailure(string email)
    {
        try
        {
            var rows = CloudSession.Anonymous(() => gateway.RpcAsync<GuardRow>("login_guard_record_failure",
                new
                {
                    p_email = email.Trim().ToLowerInvariant(),
                    p_max_attempts = LoginGuardPolicy.MaximumAttempts,
                    p_lock_minutes = LoginGuardPolicy.LockMinutes
                }, accessToken: null));
            return rows.FirstOrDefault()?.Attempts ?? 1;
        }
        catch (CloudException ex) when (ex.Failure == CloudFailure.Other)
        {
            return LoginGuardPolicy.MaximumAttempts;
        }
    }

    void TryGuard(Func<Task> operation)
    {
        try { CloudSession.FireAndWait(operation); }
        catch (CloudException) { /* la guardia es una mejora de seguridad, no bloquea el acceso */ }
    }

    ProfileRow LoadProfile(string authUserId)
    {
        if (string.IsNullOrWhiteSpace(authUserId))
            throw new CloudException(CloudFailure.Other, "Supabase no devolvio el identificador de la cuenta.");
        var profile = session.Query(token => gateway.SelectAsync<ProfileRow>("profiles",
            $"select=id,auth_user_id,full_name,email,role,active,created_at,failed_login_attempts,lockout_until&auth_user_id=eq.{Uri.EscapeDataString(authUserId)}&limit=1", token))
            .FirstOrDefault();
        if (profile is null)
            throw new CloudException(CloudFailure.Other,
                "La cuenta existe en Supabase Auth pero todavia no tiene perfil en FinOptima. El propietario del proyecto debe ejecutar cloud/supabase_schema.sql para activar el perfil automatico.");
        return profile;
    }
}
