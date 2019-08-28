using System.Collections.Generic;

namespace Core.Utilities.Init.Validation
{
  public interface IInitializersValidator
  {
    ValidationResult Validate(IReadOnlyCollection<IInitializer> initializers);
  }
}