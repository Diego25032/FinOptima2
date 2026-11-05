using FinOptima.Domain.Enums;
namespace FinOptima.Domain.Entities;
public sealed class Transaction { public int Id {get;set;} public int UserId {get;set;} public int? EnvelopeId {get;set;} public TransactionType Type {get;set;} public string Category {get;set;}=""; public string Description {get;set;}=""; public decimal Amount {get;set;} public DateTime Date {get;set;}=DateTime.Today; }
