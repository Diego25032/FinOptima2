namespace FinOptima.Application.Validators;
public sealed class ValidationResult { public bool IsValid=>Errors.Count==0; public List<string> Errors {get;}=new(); public void Add(string error)=>Errors.Add(error); }
