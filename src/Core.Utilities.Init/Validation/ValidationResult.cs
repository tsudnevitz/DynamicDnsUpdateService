using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Utilities.Init.Validation
{
  public class ValidationResult
  {
    private readonly List<KeyValuePair<string, string>> _errors;

    public ValidationResult(IEnumerable<KeyValuePair<string, string>> errors)
    {
      if (_errors == null)
        throw new ArgumentNullException(nameof(errors));

      _errors = new List<KeyValuePair<string, string>>(errors);
    }

    public ValidationResult()
    {
      _errors = new List<KeyValuePair<string, string>>();
    }

    public string ErrorMessage => ToString();
    public IEnumerable<KeyValuePair<string, string>> Errors => _errors;
    public bool IsValid => !Errors.Any();

    public void AddError(string memberName, string errorMessage)
    {
      _errors.Add(KeyValuePair.Create(memberName, errorMessage));
    }

    public override string ToString()
    {
      return string.Join(Environment.NewLine, Errors.Select(e => $"{e.Key} - {e.Value}"));
    }
  }
}