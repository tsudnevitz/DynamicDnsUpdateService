using System;

namespace Core.Utilities.Init.Validation
{
  public class ValidationException : Exception
  {
    public readonly ValidationResult Result;
    public override string Message => Result.ErrorMessage;

    public ValidationException(ValidationResult result)
    {
      Result = result ?? throw new ArgumentNullException(nameof(result));
    }
  }
}