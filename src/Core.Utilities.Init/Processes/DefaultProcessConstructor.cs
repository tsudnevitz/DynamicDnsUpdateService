using System;
using System.Collections.Generic;
using System.Linq;
using Core.Utilities.Init.Enums;
using Core.Utilities.Init.Validation;

namespace Core.Utilities.Init.Processes
{
  public class DefaultProcessConstructor : IProcessConstructor
  {
    private readonly IInitializersValidator _validator;
    private readonly IProcessOptimizer _optimizer;

    public DefaultProcessConstructor(IInitializersValidator validator, IProcessOptimizer optimizer)
    {
      _validator = validator ?? throw new ArgumentNullException(nameof(validator));
      _optimizer = optimizer ?? throw new ArgumentNullException(nameof(optimizer));
    }

    public IProcess BuildStartupProcess(IReadOnlyCollection<IInitializer> initializers)
    {
      return BuildProcess(initializers, InitializerExtractor.ForStartup());
    }

    public IProcess BuildShutdownProcess(IReadOnlyCollection<IInitializer> initializers)
    {
      return BuildProcess(initializers, InitializerExtractor.ForShutdown());
    }

    private IProcess BuildProcess(IReadOnlyCollection<IInitializer> initializers, InitializerExtractor extractor)
    {
      var result = _validator.Validate(initializers);
      if (!result.IsValid)
        throw new ValidationException(result);

      var firstInitializer = initializers.SingleOrDefault(x => extractor.WhereModeEquals(x, RunModes.First));
      var lastInitializer = initializers.SingleOrDefault(x => extractor.WhereModeEquals(x, RunModes.Last));
      var beginningInitializers = initializers.Where(x => extractor.WhereModeEquals(x, RunModes.Beginning)).ToArray();
      var endingInitializers = initializers.Where(x => extractor.WhereModeEquals(x, RunModes.Ending)).ToArray();

      var mainProcess = new SequentialProcess();

      if (firstInitializer != null)
        mainProcess.Add(extractor.AsSingleProcess(firstInitializer));

      var sequence = BuildSequence(beginningInitializers, extractor);
      if (sequence != null)
        mainProcess.Add(sequence);

      sequence = BuildSequence(endingInitializers, extractor);
      if (sequence != null)
        mainProcess.Add(sequence);

      if (lastInitializer != null)
        mainProcess.Add(extractor.AsSingleProcess(lastInitializer));

      var optimizedProcess = _optimizer.OptimizeProcess(mainProcess);
      return optimizedProcess;
    }
    
    private static IProcess BuildSequence(IReadOnlyList<IInitializer> initializers, InitializerExtractor extractor)
    {
      if (initializers.Count == 0)
        return null;

      if (initializers.Count == 1)
        return extractor.AsSingleProcess(initializers[0]);

      var dependentInitializers = initializers.Where(x => extractor.GetDependencies(x).Any(y => initializers.Any(z => y == z.GetType()))).ToArray();
      var independentInitializers = initializers.Except(dependentInitializers).ToArray();

      var mainProcess = new SequentialProcess();
      if (independentInitializers.Length > 0)
      {
        var parallelProcess = new ParallelProcess();
        parallelProcess.AddRange(independentInitializers.Select(extractor.AsSingleProcess));
        mainProcess.Add(parallelProcess);
      }

      if (dependentInitializers.Length == 0) 
        return mainProcess;

      var process = BuildSequence(dependentInitializers, extractor);
      mainProcess.Add(process);

      return mainProcess;
    }
  }
}