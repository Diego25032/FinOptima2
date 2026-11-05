using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using FinOptima.Domain.Entities;
using FinOptima.Domain.Enums;

namespace FinOptima.Infrastructure.Cloud;

/// <summary>
/// Filas tal como las entrega PostgREST (snake_case) y su conversion a las entidades
/// del dominio. Los conversores tolerantes existen porque numeric y timestamptz pueden
/// llegar como texto segun la version del servidor.
/// </summary>
public sealed class ProfileRow
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("auth_user_id")] public string AuthUserId { get; set; } = "";
    [JsonPropertyName("full_name")] public string FullName { get; set; } = "";
    [JsonPropertyName("email")] public string Email { get; set; } = "";
    [JsonPropertyName("role")] public string Role { get; set; } = "Usuario";
    [JsonPropertyName("active")] public bool Active { get; set; } = true;
    [JsonPropertyName("created_at"), JsonConverter(typeof(FlexibleDateTimeConverter))] public DateTime CreatedAt { get; set; }
    [JsonPropertyName("failed_login_attempts")] public int FailedLoginAttempts { get; set; }
    [JsonPropertyName("lockout_until"), JsonConverter(typeof(FlexibleNullableDateTimeConverter))] public DateTime? LockoutUntil { get; set; }

    public User ToEntity() => new()
    {
        Id = Id,
        AuthUserId = AuthUserId,
        FullName = FullName,
        Email = Email,
        Role = Role,
        Active = Active,
        CreatedAt = CreatedAt,
        FailedLoginAttempts = FailedLoginAttempts,
        LockoutUntil = LockoutUntil
    };
}

public sealed class EnvelopeRow
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("user_id")] public int UserId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("type")] public int Type { get; set; }
    [JsonPropertyName("target_percent"), JsonConverter(typeof(FlexibleDecimalConverter))] public decimal TargetPercent { get; set; }
    [JsonPropertyName("min_percent"), JsonConverter(typeof(FlexibleDecimalConverter))] public decimal MinPercent { get; set; }
    [JsonPropertyName("max_percent"), JsonConverter(typeof(FlexibleDecimalConverter))] public decimal MaxPercent { get; set; }
    [JsonPropertyName("balance"), JsonConverter(typeof(FlexibleDecimalConverter))] public decimal Balance { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("active")] public bool Active { get; set; } = true;

    public Envelope ToEntity() => new()
    {
        Id = Id,
        UserId = UserId,
        Name = Name,
        Type = (EnvelopeType)Type,
        TargetPercent = TargetPercent,
        MinPercent = MinPercent,
        MaxPercent = MaxPercent,
        Balance = Balance,
        Description = Description ?? "",
        Active = Active
    };

    public object ToPayload() => new
    {
        user_id = UserId,
        name = Name,
        type = Type,
        target_percent = TargetPercent,
        min_percent = MinPercent,
        max_percent = MaxPercent,
        balance = Balance,
        description = Description ?? "",
        active = Active
    };
}

public sealed class TransactionRow
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("user_id")] public int UserId { get; set; }
    [JsonPropertyName("envelope_id")] public int? EnvelopeId { get; set; }
    [JsonPropertyName("type")] public int Type { get; set; }
    [JsonPropertyName("category")] public string Category { get; set; } = "";
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("amount"), JsonConverter(typeof(FlexibleDecimalConverter))] public decimal Amount { get; set; }
    [JsonPropertyName("date"), JsonConverter(typeof(FlexibleDateTimeConverter))] public DateTime Date { get; set; }

    public Transaction ToEntity() => new()
    {
        Id = Id,
        UserId = UserId,
        EnvelopeId = EnvelopeId,
        Type = (TransactionType)Type,
        Category = Category,
        Description = Description ?? "",
        Amount = Amount,
        Date = Date
    };

    public object ToPayload() => new
    {
        user_id = UserId,
        envelope_id = EnvelopeId,
        type = Type,
        category = Category,
        description = Description ?? "",
        amount = Amount,
        date = Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
    };
}

public sealed class GoalRow
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("user_id")] public int UserId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("target_amount"), JsonConverter(typeof(FlexibleDecimalConverter))] public decimal TargetAmount { get; set; }
    [JsonPropertyName("current_amount"), JsonConverter(typeof(FlexibleDecimalConverter))] public decimal CurrentAmount { get; set; }
    [JsonPropertyName("deadline"), JsonConverter(typeof(FlexibleDateTimeConverter))] public DateTime Deadline { get; set; }
    [JsonPropertyName("priority")] public string Priority { get; set; } = "Media";

    public FinancialGoal ToEntity() => new()
    {
        Id = Id,
        UserId = UserId,
        Name = Name,
        TargetAmount = TargetAmount,
        CurrentAmount = CurrentAmount,
        Deadline = Deadline,
        Priority = Priority
    };

    public object ToPayload() => new
    {
        user_id = UserId,
        name = Name,
        target_amount = TargetAmount,
        current_amount = CurrentAmount,
        deadline = Deadline.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        priority = Priority
    };
}

public sealed class DebtRow
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("user_id")] public int UserId { get; set; }
    [JsonPropertyName("creditor")] public string Creditor { get; set; } = "";
    [JsonPropertyName("principal"), JsonConverter(typeof(FlexibleDecimalConverter))] public decimal Principal { get; set; }
    [JsonPropertyName("balance"), JsonConverter(typeof(FlexibleDecimalConverter))] public decimal Balance { get; set; }
    [JsonPropertyName("interest_rate"), JsonConverter(typeof(FlexibleDecimalConverter))] public decimal InterestRate { get; set; }
    [JsonPropertyName("minimum_payment"), JsonConverter(typeof(FlexibleDecimalConverter))] public decimal MinimumPayment { get; set; }
    [JsonPropertyName("due_date"), JsonConverter(typeof(FlexibleDateTimeConverter))] public DateTime DueDate { get; set; }

    public Debt ToEntity() => new()
    {
        Id = Id,
        UserId = UserId,
        Creditor = Creditor,
        Principal = Principal,
        Balance = Balance,
        InterestRate = InterestRate,
        MinimumPayment = MinimumPayment,
        DueDate = DueDate
    };

    public object ToPayload() => new
    {
        user_id = UserId,
        creditor = Creditor,
        principal = Principal,
        balance = Balance,
        interest_rate = InterestRate,
        minimum_payment = MinimumPayment,
        due_date = DueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
    };
}

public sealed class AuditRow
{
    [JsonPropertyName("created_at"), JsonConverter(typeof(FlexibleDateTimeConverter))] public DateTime CreatedAt { get; set; }
    [JsonPropertyName("admin_user_id")] public int AdminProfileId { get; set; }
    [JsonPropertyName("action")] public string Action { get; set; } = "";
    [JsonPropertyName("details")] public string? Details { get; set; }

    /// <summary>El correo del administrador se resuelve en memoria: audit_log solo guarda el id.</summary>
    public AuditEntry ToEntity(string? adminEmail) => new()
    {
        CreatedAt = CreatedAt,
        AdminEmail = string.IsNullOrWhiteSpace(adminEmail) ? "Cuenta eliminada" : adminEmail,
        Action = Action,
        Details = Details ?? ""
    };
}

/// <summary>Resultado de las funciones RPC de guardia de intentos.</summary>
public sealed class GuardRow
{
    [JsonPropertyName("attempts")] public int Attempts { get; set; }
    [JsonPropertyName("locked_until"), JsonConverter(typeof(FlexibleNullableDateTimeConverter))] public DateTime? LockedUntil { get; set; }
}

/// <summary>Un decimal que puede venir como numero o como texto.</summary>
public sealed class FlexibleDecimalConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType switch
    {
        JsonTokenType.Number => reader.GetDecimal(),
        JsonTokenType.String => decimal.TryParse(reader.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : 0m,
        JsonTokenType.Null => 0m,
        _ => throw new JsonException("El valor no es un numero.")
    };

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) => writer.WriteNumberValue(value);
}

/// <summary>Fecha o marca de tiempo que puede llegar con zona, sin hora, o como texto.</summary>
public sealed class FlexibleDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var text = ReadText(reader);
        if (text is null) return DateTime.MinValue;
        return Parse(text);
    }

    internal static string? ReadText(Utf8JsonReader reader) => reader.TokenType switch
    {
        JsonTokenType.String => reader.GetString(),
        JsonTokenType.Null => null,
        _ => throw new JsonException("El valor no es una fecha.")
    };

    internal static DateTime Parse(string text)
    {
        if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var datedOffset))
            return datedOffset.ToLocalTime().DateTime;
        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var plain))
            return plain;
        return DateTime.MinValue;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
}

public sealed class FlexibleNullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var text = FlexibleDateTimeConverter.ReadText(reader);
        if (string.IsNullOrWhiteSpace(text)) return null;
        var parsed = FlexibleDateTimeConverter.Parse(text);
        return parsed == DateTime.MinValue ? null : parsed;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue) writer.WriteStringValue(value.Value.ToString("O", CultureInfo.InvariantCulture));
        else writer.WriteNullValue();
    }
}
