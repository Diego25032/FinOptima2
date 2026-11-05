namespace FinOptima.Application.DTOs;
public sealed class DashboardDto { public decimal Income {get;set;} public decimal Expenses {get;set;} public decimal Available => Income-Expenses; public decimal SavingsRate => Income==0?0:(Available/Income)*100; public int Goals {get;set;} public int Debts {get;set;} public Dictionary<string,decimal> EnvelopeBalances {get;set;}=new(); }
