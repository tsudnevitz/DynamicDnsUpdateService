using System.Collections.Generic;
using System.Linq;
using Core.Utilities.Init.Enums;

namespace Core.Utilities.Init.Validation
{
  public class DefaultInitializersValidator : IInitializersValidator
  {
    public ValidationResult Validate(IReadOnlyCollection<IInitializer> initializers)
    {
      var result = new ValidationResult();

      var startFirstInitializer = initializers.SingleOrDefault(x => x.StartMode == RunModes.First);
      var startLastInitializer = initializers.SingleOrDefault(x => x.StartMode == RunModes.Last);
      var startBeginningInitializers = initializers.Where(x => x.StartMode == RunModes.Beginning);
      var startEndingInitializers = initializers.Where(x => x.StartMode == RunModes.Ending);
      var stopFirstInitializer = initializers.SingleOrDefault(x => x.StartMode == RunModes.First);
      var stopLastInitializer = initializers.SingleOrDefault(x => x.StartMode == RunModes.Last);
      var stopBeginningInitializers = initializers.Where(x => x.StartMode == RunModes.Beginning);
      var stopEndingInitializers = initializers.Where(x => x.StartMode == RunModes.Ending);

      // startup
      if (initializers.Count(x => x.StartMode == RunModes.First) > 1)
        result.AddError("StartMode", "More then one initializer defined as first to run on startup sequence.");

      if (initializers.Count(x => x.StartMode == RunModes.Last) > 1)
        result.AddError("StartMode", "More then one initializer defined as last to run on startup sequence.");

      if (startFirstInitializer?.OnStartRunAfter.Any() ?? false)
        result.AddError("OnStartRunAfter", "First initializer is declared to run after some other initializer.");

      if (startLastInitializer != null && initializers.Any(x => x.OnStartRunAfter.Any(y => y == startLastInitializer.GetType())))
        result.AddError("OnStartRunAfter", "One or more initializers have declared to run after last initializer.");
      
      if (startBeginningInitializers.Any(x => x.OnStartRunAfter.Any(y => startEndingInitializers.Any(z => y == z.GetType()))))
        result.AddError("OnStartRunAfter", "One or more beginning initializers are declared to run after ending initializers.");

      // shutdown
      if (initializers.Count(x => x.StopMode == RunModes.First) > 1)
        result.AddError("StopMode", "More then one initializer defined as first to run on shutdown sequence.");

      if (initializers.Count(x => x.StopMode == RunModes.Last) > 1)
        result.AddError("StopMode", "More then one initializer defined as last to run on shutdown sequence.");

      if (stopFirstInitializer?.OnStopRunAfter.Any() ?? false)
        result.AddError("OnStopRunAfter", "First initializer is declared to run after some other initializer.");

      if (stopLastInitializer != null && initializers.Any(x => x.OnStopRunAfter.Any(y => y == stopLastInitializer.GetType())))
        result.AddError("OnStopRunAfter", "One or more initializers have declared to run after last initializer.");
      
      if (stopBeginningInitializers.Any(x => x.OnStopRunAfter.Any(y => stopEndingInitializers.Any(z => y == z.GetType()))))
        result.AddError("OnStopRunAfter", "One or more beginning initializers are declared to run after ending initializers.");

      // ToDo: cycles detection for beginning and ending
      return result;
    }
  }
}