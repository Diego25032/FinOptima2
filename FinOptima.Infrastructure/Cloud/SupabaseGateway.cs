using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinOptima.Infrastructure.Cloud;

/// <summary>
/// Cliente HTTP minimo contra Supabase: Auth (GoTrue) para las cuentas y PostgREST
/// para los datos. Sin paquetes externos, para que la solucion compile en cualquier
/// equipo con el SDK de .NET 8.
/// Cada peticion de datos usa el token de la persona conectada: quien separa los
/// datos por usuario es Row Level Security en el servidor, no este cliente.
/// </summary>
public sealed class SupabaseGateway
{
    static readonly HttpClient Http = CreateHttpClient();
    static readonly JsonSerializerOptions JsonOptions = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    public SupabaseSettings Settings { get; }

    public SupabaseGateway(SupabaseSettings settings)
    {
        Settings = settings;
        if (!settings.IsConfigured)
            throw new CloudException(CloudFailure.Network, "FinOptima todavia no esta conectado a Supabase. Abre Configurar conexion e ingresa la URL y la clave publica del proyecto.");
        if (!settings.ProjectUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            throw new CloudException(CloudFailure.Network, "La URL del proyecto debe empezar por https:// para que la conexion vaya cifrada.");
    }

    static HttpClient CreateHttpClient() => new() { Timeout = TimeSpan.FromSeconds(30) };

    // ---------------------------------------------------------------- Auth

    public async Task<AuthTokens> SignInAsync(string email, string password)
    {
        var json = await RequestAsync(HttpMethod.Post, "/auth/v1/token?grant_type=password",
            Serialize(new { email, password }), accessToken: null).ConfigureAwait(false);
        var parsed = FromJson<AuthResponseDto>(json) ?? throw new CloudException(CloudFailure.Other, "Supabase devolvio una respuesta sin contenido.");
        if (string.IsNullOrWhiteSpace(parsed.AccessToken) || string.IsNullOrWhiteSpace(parsed.RefreshToken))
            throw new CloudException(CloudFailure.Other, "Supabase no devolvio una sesion valida para esa cuenta.");
        return new AuthTokens(
            parsed.AccessToken,
            parsed.RefreshToken,
            DateTimeOffset.UtcNow.AddSeconds(Math.Clamp(parsed.ExpiresIn ?? 3600, 60, 86_400)),
            parsed.User?.Id ?? parsed.Id ?? "",
            parsed.User?.Email ?? parsed.Email ?? email);
    }

    /// <summary>
    /// Registro publico. Si el proyecto exige confirmacion por correo, Supabase responde
    /// solo con el usuario y sin sesion; en ese caso RequiresConfirmation queda en true.
    /// </summary>
    public async Task<AuthTokens> SignUpAsync(string email, string password, string fullName)
    {
        var json = await RequestAsync(HttpMethod.Post, "/auth/v1/signup",
            Serialize(new { email, password, data = new { full_name = fullName } }), accessToken: null).ConfigureAwait(false);
        var parsed = FromJson<AuthResponseDto>(json) ?? throw new CloudException(CloudFailure.Other, "Supabase devolvio una respuesta sin contenido.");
        var authUserId = parsed.User?.Id ?? parsed.Id ?? "";
        var mail = parsed.User?.Email ?? parsed.Email ?? email;
        if (string.IsNullOrWhiteSpace(parsed.AccessToken) || string.IsNullOrWhiteSpace(parsed.RefreshToken))
            return new AuthTokens("", "", DateTimeOffset.MinValue, authUserId, mail) { RequiresConfirmation = true };
        return new AuthTokens(
            parsed.AccessToken,
            parsed.RefreshToken,
            DateTimeOffset.UtcNow.AddSeconds(Math.Clamp(parsed.ExpiresIn ?? 3600, 60, 86_400)),
            authUserId, mail);
    }

    public async Task<AuthTokens> RefreshAsync(string refreshToken)
    {
        var json = await RequestAsync(HttpMethod.Post, "/auth/v1/token?grant_type=refresh_token",
            Serialize(new { refresh_token = refreshToken }), accessToken: null).ConfigureAwait(false);
        var parsed = FromJson<AuthResponseDto>(json) ?? throw new CloudException(CloudFailure.Other, "Supabase devolvio una respuesta sin contenido.");
        if (string.IsNullOrWhiteSpace(parsed.AccessToken))
            throw new CloudException(CloudFailure.Permission, "La sesion expiro y Supabase no quiso renovarla. Cierra sesion y vuelve a entrar.");
        return new AuthTokens(
            parsed.AccessToken,
            parsed.RefreshToken ?? refreshToken,
            DateTimeOffset.UtcNow.AddSeconds(Math.Clamp(parsed.ExpiresIn ?? 3600, 60, 86_400)),
            parsed.User?.Id ?? "", parsed.User?.Email ?? "");
    }

    /// <summary>Cerrar sesion en el servidor. Si el token ya no sirve, el error se ignora: el borrado local es lo que importa.</summary>
    public async Task SignOutAsync(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken)) return;
        try { await RequestAsync(HttpMethod.Post, "/auth/v1/logout", "{}", accessToken).ConfigureAwait(false); }
        catch (CloudException) { }
    }

    public async Task UpdatePasswordAsync(string accessToken, string newPassword)
    {
        await RequestAsync(HttpMethod.Put, "/auth/v1/user", Serialize(new { password = newPassword }), accessToken).ConfigureAwait(false);
    }

    // ---------------------------------------------------------------- PostgREST

    public async Task<List<T>> SelectAsync<T>(string table, string query, string accessToken)
    {
        var json = await RequestAsync(HttpMethod.Get, $"/rest/v1/{table}?{query}", body: null, accessToken).ConfigureAwait(false);
        return FromJson<List<T>>(json) ?? new List<T>();
    }

    /// <summary>Inserta una fila y devuelve la fila que genero el servidor (incluye el id).</summary>
    public async Task<T> InsertAsync<T>(string table, object row, string accessToken)
    {
        var json = await RequestAsync(HttpMethod.Post, $"/rest/v1/{table}", Serialize(row), accessToken, prefer: "return=representation").ConfigureAwait(false);
        var created = FromJson<List<T>>(json);
        if (created is null || created.Count == 0)
            throw new CloudException(CloudFailure.Other, $"Supabase guardo el registro en {table} pero no devolvio su identificador.");
        return created[0];
    }

    public async Task UpdateAsync(string table, object changes, string filter, string accessToken)
    {
        await RequestAsync(new HttpMethod("PATCH"), $"/rest/v1/{table}?{filter}", Serialize(changes), accessToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(string table, string filter, string accessToken)
    {
        await RequestAsync(HttpMethod.Delete, $"/rest/v1/{table}?{filter}", body: null, accessToken).ConfigureAwait(false);
    }

    /// <summary>Funcion RPC que devuelve filas o un valor.</summary>
    public async Task<List<T>> RpcAsync<T>(string function, object arguments, string? accessToken)
    {
        var json = await RequestAsync(HttpMethod.Post, $"/rest/v1/rpc/{function}", Serialize(arguments), accessToken, prefer: "return=representation").ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json)) return new List<T>();
        if (json.TrimStart().StartsWith('[')) return FromJson<List<T>>(json) ?? new List<T>();
        var single = FromJson<T>(json);
        return single is null ? new List<T>() : new List<T> { single };
    }

    /// <summary>Funcion RPC sin resultado (por ejemplo admin_update_profile).</summary>
    public async Task RpcAsync(string function, object arguments, string? accessToken)
    {
        await RequestAsync(HttpMethod.Post, $"/rest/v1/rpc/{function}", Serialize(arguments), accessToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Solo para diagnostico: /auth/v1/settings es publico y responde solo si la URL existe
    /// y la clave del proyecto es aceptada, sin leer datos de nadie.
    /// </summary>
    public async Task<string> ProbeAsync()
    {
        return await RequestAsync(HttpMethod.Get, "/auth/v1/settings", body: null, accessToken: null).ConfigureAwait(false);
    }

    // ---------------------------------------------------------------- filtros y JSON

    /// <summary>Arma `columna=eq.valor` escapando el valor. Solo para numeros, ids y banderas.</summary>
    public static string Eq(string column, object value)
    {
        var text = value switch
        {
            bool flag => flag ? "true" : "false",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? ""
        };
        return $"{column}=eq.{Uri.EscapeDataString(text)}";
    }

    static string Serialize(object payload) => JsonSerializer.Serialize(payload, JsonOptions);

    static T? FromJson<T>(string json)
    {
        try { return JsonSerializer.Deserialize<T>(json, JsonOptions); }
        catch (JsonException ex)
        {
            throw new CloudException(CloudFailure.Other, "La respuesta de Supabase no tiene el formato esperado.", ex.GetType().Name)
            { TechnicalMessage = json.Length > 300 ? json[..300] : json };
        }
    }

    async Task<string> RequestAsync(HttpMethod method, string pathAndQuery, string? body, string? accessToken, string? prefer = null)
    {
        HttpResponseMessage response;
        try
        {
            using var request = new HttpRequestMessage(method, Settings.ProjectUrl + pathAndQuery);
            request.Headers.TryAddWithoutValidation("apikey", Settings.AnonKey);
            if (!string.IsNullOrWhiteSpace(accessToken)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (!string.IsNullOrWhiteSpace(prefer)) request.Headers.TryAddWithoutValidation("Prefer", prefer);
            if (body is not null) request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            response = await Http.SendAsync(request).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw new CloudException(CloudFailure.Network, "Supabase tardo demasiado en responder. Revisa tu conexion a internet y vuelve a intentarlo.", "timeout");
        }
        catch (HttpRequestException ex)
        {
            throw new CloudException(CloudFailure.Network, "No se pudo conectar con Supabase. Verifica la URL del proyecto y tu conexion a internet.", ex.GetType().Name)
            { TechnicalMessage = ex.Message };
        }

        using (response)
        {
            var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return text;
            throw BuildException(response.StatusCode, text);
        }
    }

    /// <summary>
    /// Traduce GoTrue/PostgREST a un mensaje claro para el usuario. El cuerpo del servidor
    /// se guarda aparte para diagnostico y nunca se muestra tal cual: puede contener
    /// fragmentos del registro o de la base de datos.
    /// </summary>
    static CloudException BuildException(HttpStatusCode status, string body)
    {
        var serverMessage = ExtractMessage(body) ?? "";
        var code = ExtractCode(body);
        var lowered = serverMessage.ToLowerInvariant();

        CloudFailure failure;
        string message;
        if (lowered.Contains("invalid login credentials") || lowered.Contains("invalid credentials") || lowered.Contains("wrong password") || lowered.Contains("user not found"))
        {
            failure = CloudFailure.Credentials;
            message = "Correo o contraseña incorrectos.";
        }
        else if (lowered.Contains("email not confirmed") || lowered.Contains("confirm your email"))
        {
            failure = CloudFailure.NotConfirmed;
            message = "Ese correo todavia no esta confirmado. Abre el mensaje de Supabase o pide al propietario del proyecto desactivar la confirmacion automatica.";
        }
        else if (status == HttpStatusCode.TooManyRequests || lowered.Contains("rate limit") || lowered.Contains("too many requests") || lowered.Contains("for this hour"))
        {
            failure = CloudFailure.RateLimited;
            message = "Supabase limito los intentos por ahora. Espera unos minutos y vuelve a intentar.";
        }
        else if (lowered.Contains("reauthentication required") || lowered.Contains("reauthenticate"))
        {
            failure = CloudFailure.Permission;
            message = "Supabase pide verificar de nuevo tu contraseña antes de cambiarla. Cierra sesion, vuelve a entrar y repite el cambio.";
        }
        else if (lowered.Contains("password should be") || lowered.Contains("does not meet password") || lowered.Contains("weak_password"))
        {
            failure = CloudFailure.Credentials;
            message = "La contraseña no cumple la politica minima configurada en Supabase.";
        }
        else if (string.Equals(code, "23505", StringComparison.Ordinal) || lowered.Contains("duplicate key"))
        {
            failure = CloudFailure.Conflict;
            message = "Ese correo ya esta registrado en FinOptima.";
        }
        else if (string.Equals(code, "23503", StringComparison.Ordinal) || lowered.Contains("foreign key"))
        {
            failure = CloudFailure.Conflict;
            message = "El registro hace referencia a un dato que ya no existe. Actualiza la pantalla e intenta de nuevo.";
        }
        else if (lowered.Contains("row-level security"))
        {
            failure = CloudFailure.Permission;
            message = "Las reglas de seguridad del proyecto no permiten guardar esos datos en la cuenta de otra persona.";
        }
        else if (status is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            failure = CloudFailure.Permission;
            message = "La sesion perdio permisos en el servidor. Cierra sesion y vuelve a entrar.";
        }
        else if (status == HttpStatusCode.NotFound)
        {
            failure = CloudFailure.Other;
            message = "Supabase no encontro esa tabla o funcion. Ejecuta otra vez el esquema del proyecto.";
        }
        else
        {
            failure = CloudFailure.Other;
            message = "Supabase rechazo la operacion. Revisa los datos e intenta de nuevo.";
        }

        return new CloudException(failure, message, code) { TechnicalMessage = $"{(int)status} {status}: {serverMessage}" };
    }

    static string? ExtractMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;
        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.ValueKind == JsonValueKind.String) return document.RootElement.GetString();
            if (document.RootElement.ValueKind != JsonValueKind.Object) return null;
            foreach (var name in new[] { "msg", "error_description", "error", "message", "detail" })
            {
                if (!document.RootElement.TryGetProperty(name, out var value)) continue;
                if (value.ValueKind == JsonValueKind.String) return value.GetString();
                if (value.ValueKind == JsonValueKind.Object && value.TryGetProperty("message", out var nested)) return nested.GetString();
            }
            return null;
        }
        catch (JsonException) { return body.Length > 200 ? body[..200] : body; }
    }

    static string? ExtractCode(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.ValueKind == JsonValueKind.Object && document.RootElement.TryGetProperty("code", out var code))
                return code.ValueKind == JsonValueKind.String ? code.GetString() : code.ToString();
        }
        catch { }
        return null;
    }
}

public sealed record AuthTokens(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt, string AuthUserId, string Email)
{
    public bool RequiresConfirmation { get; init; }

    [JsonIgnore]
    public bool HasSession => !string.IsNullOrWhiteSpace(AccessToken) && !string.IsNullOrWhiteSpace(RefreshToken);
}

internal sealed class AuthResponseDto
{
    [JsonPropertyName("access_token")] public string? AccessToken { get; set; }
    [JsonPropertyName("refresh_token")] public string? RefreshToken { get; set; }
    [JsonPropertyName("expires_in")] public int? ExpiresIn { get; set; }
    [JsonPropertyName("user")] public AuthUserDto? User { get; set; }

    // Cuando el registro exige confirmacion, Supabase responde con el usuario en la raiz.
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("email")] public string? Email { get; set; }
}

internal sealed class AuthUserDto
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("email")] public string? Email { get; set; }
}
