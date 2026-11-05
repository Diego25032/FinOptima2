namespace FinOptima.Domain.Entities;
public sealed class FinancialPlan { public int Id {get;set;} public int UserId {get;set;} public string Month {get;set;}=""; public decimal Income {get;set;} public decimal Allocated {get;set;} public DateTime CreatedAt {get;set;}=DateTime.Now; }
