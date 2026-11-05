namespace FinOptima.Domain.Entities;
public sealed class Debt { public int Id {get;set;} public int UserId {get;set;} public string Creditor {get;set;}=""; public decimal Principal {get;set;} public decimal Balance {get;set;} public decimal InterestRate {get;set;} public decimal MinimumPayment {get;set;} public DateTime DueDate {get;set;} public bool PaidOff => Balance<=0; }
