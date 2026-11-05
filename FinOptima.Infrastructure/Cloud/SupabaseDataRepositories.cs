using FinOptima.Application.Interfaces;
using FinOptima.Domain.Entities;

namespace FinOptima.Infrastructure.Cloud;

/// <summary>
/// Repositorios sobre la API REST de Supabase. Las sentencias SQL desaparecen: cada
/// operacion es una peticion a PostgREST con el token de la persona conectada, y el
/// servidor decide por Row Level Security que solo se ven y se tocan filas propias.
/// </summary>
public sealed class SupabaseEnvelopeRepository(CloudSession session, SupabaseGateway gateway) : IEnvelopeRepository
{
    public List<Envelope> GetByUser(int userId) =>
        session.Query(token => gateway.SelectAsync<EnvelopeRow>("envelopes",
            $"select=*&user_id=eq.{userId}&active=eq.true&order=id.asc", token)).ConvertAll(row => row.ToEntity());

    public Envelope Add(Envelope envelope)
    {
        var row = new EnvelopeRow
        {
            UserId = envelope.UserId,
            Name = envelope.Name,
            Type = (int)envelope.Type,
            TargetPercent = envelope.TargetPercent,
            MinPercent = envelope.MinPercent,
            MaxPercent = envelope.MaxPercent,
            Balance = envelope.Balance,
            Description = envelope.Description,
            Active = true
        };
        var created = session.Query(token => gateway.InsertAsync<EnvelopeRow>("envelopes", row.ToPayload(), token));
        envelope.Id = created.Id;
        return envelope;
    }

    public void Update(Envelope envelope) =>
        session.Execute(token => gateway.UpdateAsync("envelopes", new EnvelopeRow
        {
            UserId = envelope.UserId,
            Name = envelope.Name,
            Type = (int)envelope.Type,
            TargetPercent = envelope.TargetPercent,
            MinPercent = envelope.MinPercent,
            MaxPercent = envelope.MaxPercent,
            Balance = envelope.Balance,
            Description = envelope.Description,
            Active = envelope.Active
        }.ToPayload(), $"id=eq.{envelope.Id}&user_id=eq.{envelope.UserId}", token));

    /// <summary>Borrado logico: el sobre deja de aparecer pero los movimientos siguen apuntando a el.</summary>
    public void Delete(int id, int userId) =>
        session.Execute(token => gateway.UpdateAsync("envelopes", new { active = false }, $"id=eq.{id}&user_id=eq.{userId}", token));
}

public sealed class SupabaseTransactionRepository(CloudSession session, SupabaseGateway gateway) : ITransactionRepository
{
    public List<Transaction> GetByUser(int userId) =>
        session.Query(token => gateway.SelectAsync<TransactionRow>("transactions",
            $"select=*&user_id=eq.{userId}&order=date.desc,id.desc", token)).ConvertAll(row => row.ToEntity());

    public Transaction Add(Transaction transaction)
    {
        var row = new TransactionRow
        {
            UserId = transaction.UserId,
            EnvelopeId = transaction.EnvelopeId,
            Type = (int)transaction.Type,
            Category = transaction.Category,
            Description = transaction.Description,
            Amount = transaction.Amount,
            Date = transaction.Date
        };
        var created = session.Query(token => gateway.InsertAsync<TransactionRow>("transactions", row.ToPayload(), token));
        transaction.Id = created.Id;
        return transaction;
    }

    public void Update(Transaction transaction) =>
        session.Execute(token => gateway.UpdateAsync("transactions", new TransactionRow
        {
            UserId = transaction.UserId,
            EnvelopeId = transaction.EnvelopeId,
            Type = (int)transaction.Type,
            Category = transaction.Category,
            Description = transaction.Description,
            Amount = transaction.Amount,
            Date = transaction.Date
        }.ToPayload(), $"id=eq.{transaction.Id}&user_id=eq.{transaction.UserId}", token));

    public void Delete(int id, int userId) =>
        session.Execute(token => gateway.DeleteAsync("transactions", $"id=eq.{id}&user_id=eq.{userId}", token));
}

public sealed class SupabaseGoalRepository(CloudSession session, SupabaseGateway gateway) : IGoalRepository
{
    public List<FinancialGoal> GetByUser(int userId) =>
        session.Query(token => gateway.SelectAsync<GoalRow>("goals", $"select=*&user_id=eq.{userId}&order=deadline.asc", token))
            .ConvertAll(row => row.ToEntity());

    public FinancialGoal Add(FinancialGoal goal)
    {
        var row = ToRow(goal);
        var created = session.Query(token => gateway.InsertAsync<GoalRow>("goals", row.ToPayload(), token));
        goal.Id = created.Id;
        return goal;
    }

    public void Update(FinancialGoal goal) =>
        session.Execute(token => gateway.UpdateAsync("goals", ToRow(goal).ToPayload(), $"id=eq.{goal.Id}&user_id=eq.{goal.UserId}", token));

    public void Delete(int id, int userId) =>
        session.Execute(token => gateway.DeleteAsync("goals", $"id=eq.{id}&user_id=eq.{userId}", token));

    static GoalRow ToRow(FinancialGoal goal) => new()
    {
        UserId = goal.UserId,
        Name = goal.Name,
        TargetAmount = goal.TargetAmount,
        CurrentAmount = goal.CurrentAmount,
        Deadline = goal.Deadline,
        Priority = goal.Priority
    };
}

public sealed class SupabaseDebtRepository(CloudSession session, SupabaseGateway gateway) : IDebtRepository
{
    public List<Debt> GetByUser(int userId) =>
        session.Query(token => gateway.SelectAsync<DebtRow>("debts", $"select=*&user_id=eq.{userId}&order=due_date.asc", token))
            .ConvertAll(row => row.ToEntity());

    public Debt Add(Debt debt)
    {
        var created = session.Query(token => gateway.InsertAsync<DebtRow>("debts", ToRow(debt).ToPayload(), token));
        debt.Id = created.Id;
        return debt;
    }

    public void Update(Debt debt) =>
        session.Execute(token => gateway.UpdateAsync("debts", ToRow(debt).ToPayload(), $"id=eq.{debt.Id}&user_id=eq.{debt.UserId}", token));

    public void Delete(int id, int userId) =>
        session.Execute(token => gateway.DeleteAsync("debts", $"id=eq.{id}&user_id=eq.{userId}", token));

    static DebtRow ToRow(Debt debt) => new()
    {
        UserId = debt.UserId,
        Creditor = debt.Creditor,
        Principal = debt.Principal,
        Balance = debt.Balance,
        InterestRate = debt.InterestRate,
        MinimumPayment = debt.MinimumPayment,
        DueDate = debt.DueDate
    };
}
