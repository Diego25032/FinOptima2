using System.Text.Json;
using System.Text.Json.Serialization;
using FinOptima.Application.Interfaces;

namespace FinOptima.Infrastructure.Cloud;

/// <summary>
/// Exporta e importa la informacion financiera de la cuenta conectada en un archivo JSON.
/// Sustituye las antiguas copias del archivo SQLite: en el modo nube la base es compartida
/// y lo que tiene valor para una persona es su propio conjunto de sobres, movimientos,
/// metas y deudas.
/// </summary>
public sealed class SupabaseDataExchangeService(CloudSession session, SupabaseGateway gateway) : IDataExchangeService
{
    static readonly JsonSerializerOptions FileJson = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string ExportAccount(int profileId)
    {
        var payload = new AccountPayload
        {
            Version = 1,
            ExportedAt = DateTime.Now,
            AccountEmail = session.Email,
            Envelopes = Read<EnvelopeRow>("envelopes", "select=*&order=id.asc", profileId),
            Transactions = Read<TransactionRow>("transactions", "select=*&order=id.asc", profileId),
            Goals = Read<GoalRow>("goals", "select=*&order=id.asc", profileId),
            Debts = Read<DebtRow>("debts", "select=*&order=id.asc", profileId)
        };
        foreach (var envelope in payload.Envelopes) envelope.UserId = 0;
        foreach (var transaction in payload.Transactions) { transaction.UserId = 0; transaction.Id = 0; }
        foreach (var goal in payload.Goals) goal.Id = 0;
        foreach (var debt in payload.Debts) debt.Id = 0;
        return JsonSerializer.Serialize(payload, FileJson);
    }

    public ImportPreview Preview(string payload) => Count(Parse(payload));

    public ImportPreview Import(string payload)
    {
        var data = Parse(payload);
        int profileId = session.ProfileId;
        if (profileId <= 0)
            throw new CloudException(CloudFailure.Permission, "Inicia sesion antes de importar una copia de seguridad.");

        // Hijos primero: los movimientos apuntan a los sobres que estan a punto de borrarse.
        session.Execute(token => gateway.DeleteAsync("transactions", $"user_id=eq.{profileId}", token));
        session.Execute(token => gateway.DeleteAsync("goals", $"user_id=eq.{profileId}", token));
        session.Execute(token => gateway.DeleteAsync("debts", $"user_id=eq.{profileId}", token));
        session.Execute(token => gateway.DeleteAsync("envelopes", $"user_id=eq.{profileId}", token));

        var rebuiltIds = new Dictionary<int, int>();
        foreach (var envelope in data.Envelopes)
        {
            envelope.UserId = profileId;
            var oldId = envelope.Id;
            var created = session.Query(token => gateway.InsertAsync<EnvelopeRow>("envelopes", envelope.ToPayload(), token));
            if (oldId > 0) rebuiltIds[oldId] = created.Id;
        }

        foreach (var transaction in data.Transactions)
        {
            transaction.UserId = profileId;
            if (transaction.EnvelopeId is int source && rebuiltIds.TryGetValue(source, out var replacement))
                transaction.EnvelopeId = replacement;
            else
                transaction.EnvelopeId = null;
            session.Query(token => gateway.InsertAsync<TransactionRow>("transactions", transaction.ToPayload(), token));
        }

        foreach (var goal in data.Goals)
        {
            goal.UserId = profileId;
            session.Query(token => gateway.InsertAsync<GoalRow>("goals", goal.ToPayload(), token));
        }

        foreach (var debt in data.Debts)
        {
            debt.UserId = profileId;
            session.Query(token => gateway.InsertAsync<DebtRow>("debts", debt.ToPayload(), token));
        }

        return Count(data);
    }

    List<T> Read<T>(string table, string query, int profileId) =>
        session.Query(token => gateway.SelectAsync<T>(table, $"{query}&user_id=eq.{profileId}", token));

    static AccountPayload Parse(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            throw new CloudException(CloudFailure.Other, "El archivo de copia de seguridad esta vacio.");
        AccountPayload? data;
        try { data = JsonSerializer.Deserialize<AccountPayload>(payload, FileJson); }
        catch (JsonException ex)
        {
            throw new CloudException(CloudFailure.Other, "El archivo no es una copia de seguridad de FinOptima en formato JSON.", ex.GetType().Name);
        }
        return data ?? throw new CloudException(CloudFailure.Other, "El archivo no es una copia de seguridad de FinOptima en formato JSON.");
    }

    static ImportPreview Count(AccountPayload data) =>
        new(data.Envelopes.Count, data.Transactions.Count, data.Goals.Count, data.Debts.Count);

    sealed class AccountPayload
    {
        [JsonPropertyName("version")] public int Version { get; set; } = 1;
        [JsonPropertyName("exported_at")] public DateTime ExportedAt { get; set; }
        [JsonPropertyName("account_email")] public string AccountEmail { get; set; } = "";
        [JsonPropertyName("envelopes")] public List<EnvelopeRow> Envelopes { get; set; } = new();
        [JsonPropertyName("transactions")] public List<TransactionRow> Transactions { get; set; } = new();
        [JsonPropertyName("goals")] public List<GoalRow> Goals { get; set; } = new();
        [JsonPropertyName("debts")] public List<DebtRow> Debts { get; set; } = new();
    }
}
