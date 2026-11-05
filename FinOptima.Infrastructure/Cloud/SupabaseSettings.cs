using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinOptima.Infrastructure.Cloud;

/// <summary>
/// URL y clave publica (anon/publishable) del proyecto Supabase de FinOptima.
/// Se guardan por maquina en %LOCALAPPDATA%\FinOptima\supabase.json y nunca se
/// distribuyen dentro del codigo ni del ZIP. Las variables de entorno
/// FINOPTIMA_SUPABASE_URL y FINOPTIMA_SUPABASE_ANON_KEY tienen prioridad.
/// </summary>
public sealed class SupabaseSettings
{
    public string ProjectUrl { get; set; } = "";
    public string AnonKey { get; set; } = "";

    [JsonIgnore]
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ProjectUrl) && !string.IsNullOrWhiteSpace(AnonKey);

    public static string ConfigDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FinOptima");

    public static string ConfigPath => Path.Combine(ConfigDirectory, "supabase.json");

    static readonly JsonSerializerOptions IO = new() { WriteIndented = true };

    public static SupabaseSettings Load()
    {
        var settings = new SupabaseSettings();
        if (File.Exists(ConfigPath))
        {
            try
            {
                var stored = JsonSerializer.Deserialize<SupabaseSettings>(File.ReadAllText(ConfigPath));
                if (stored is not null)
                {
                    settings.ProjectUrl = stored.ProjectUrl ?? "";
                    settings.AnonKey = stored.AnonKey ?? "";
                }
            }
            catch { /* archivo corrupto: se trata como sin configurar */ }
        }

        var envUrl = Environment.GetEnvironmentVariable("FINOPTIMA_SUPABASE_URL");
        var envKey = Environment.GetEnvironmentVariable("FINOPTIMA_SUPABASE_ANON_KEY");
        if (!string.IsNullOrWhiteSpace(envUrl)) settings.ProjectUrl = envUrl;
        if (!string.IsNullOrWhiteSpace(envKey)) settings.AnonKey = envKey;

        settings.ProjectUrl = NormalizeUrl(settings.ProjectUrl);
        settings.AnonKey = settings.AnonKey.Trim();
        return settings;
    }

    public void Save()
    {
        ProjectUrl = NormalizeUrl(ProjectUrl);
        AnonKey = AnonKey.Trim();
        Directory.CreateDirectory(ConfigDirectory);
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(this, IO));
    }

    public static string NormalizeUrl(string url)
    {
        url = (url ?? "").Trim().TrimEnd('/');
        if (url.Length > 0 && !url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            url = "https://" + url;
        return url;
    }

    /// <summary>Devuelve un error legible o null cuando los datos parecen validos.</summary>
    public static string? Validate(string url, string key)
    {
        url = NormalizeUrl(url);
        key = (key ?? "").Trim();
        if (url.Length == 0) return "Falta la URL del proyecto (por ejemplo https://xxxx.supabase.co).";
        if (!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return "La URL del proyecto debe empezar por https://";
        if (!url.Contains('.')) return "La URL del proyecto no parece valida.";
        if (key.Length == 0) return "Falta la clave publica del proyecto.";
        if (key.Contains(' ')) return "La clave no debe contener espacios.";
        // Las claves service_role / secret nunca deben configurarse en un cliente de escritorio:
        // saltarian el Row Level Security y darian acceso a los datos de todos los usuarios.
        if (key.StartsWith("sb_secret_", StringComparison.Ordinal)) return "Esa es una clave secreta (service_role). FinOptima solo usa la clave publica, porque la secreta romperia el aislamiento por usuario.";
        if (key.StartsWith("eyJ", StringComparison.Ordinal)) return null;
        if (key.StartsWith("sb_publishable_", StringComparison.Ordinal)) return null;
        return "La clave publica debe ser la anon key (empieza por eyJ...) o la publishable key (empieza por sb_publishable_...).";
    }
}
