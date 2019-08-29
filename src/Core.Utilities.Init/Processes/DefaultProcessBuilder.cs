using System.Collections.Generic;
using System.Linq;
using Core.Utilities.Init.Enums;

namespace Core.Utilities.Init.Processes
{
  public class DefaultProcessBuilder : IProcessConstructor
  {
    private readonly IProcessOptimizer _optimizer;

    public DefaultProcessBuilder()
    {
      _optimizer = new DefaultProcessOptimizer();
    }

    public IProcess BuildStartupProcess(IReadOnlyCollection<IInitializer> initializers)
    {
      return BuildProcess(initializers, InitializerConverter.ForStartup());
    }

    public IProcess BuildShutdownProcess(IReadOnlyCollection<IInitializer> initializers)
    {
      return BuildProcess(initializers, InitializerConverter.ForShutdown());
    }

    private IProcess BuildProcess(IReadOnlyCollection<IInitializer> initializers, InitializerConverter converter)
    {
      switch (initializers.Count)
      {
        case 0:
          return EmptyProcess.Instance;
        case 1:
          return converter.AsSingleProcess(initializers.First());
        default:
          return BuildComplexProcess(initializers, converter);
      }
    }

    private IProcess BuildComplexProcess(IReadOnlyCollection<IInitializer> initializers, InitializerConverter converter)
    {
      var firstInitializer = initializers.SingleOrDefault(x => converter.WhereModeEquals(x, RunModes.First));
      var lastInitializer = initializers.SingleOrDefault(x => converter.WhereModeEquals(x, RunModes.Last));
      var beginningInitializers = initializers.Where(x => converter.WhereModeEquals(x, RunModes.Beginning)).ToArray();
      var endingInitializers = initializers.Where(x => converter.WhereModeEquals(x, RunModes.Ending)).ToArray();

      var mainProcess = new SequentialProcess();

      if (firstInitializer != null)
        mainProcess.Add(converter.AsSingleProcess(firstInitializer));

      var sequence = BuildSequence(beginningInitializers, converter);
      if (sequence != null)
        mainProcess.Add(sequence);

      sequence = BuildSequence(endingInitializers, converter);
      if (sequence != null)
        mainProcess.Add(sequence);

      if (lastInitializer != null)
        mainProcess.Add(converter.AsSingleProcess(lastInitializer));

      var optimizedProcess = _optimizer.OptimizeProcess(mainProcess);
      return optimizedProcess;
    }

    private static IProcess BuildSequence(IReadOnlyList<IInitializer> initializers, InitializerConverter converter)
    {
      if (initializers.Count == 0)
        return null;

      if (initializers.Count == 1)
        return converter.AsSingleProcess(initializers[0]);

      var dependentInitializers = initializers.Where(x => converter.GetDependencies(x).Any(y => initializers.Any(z => y == z.GetType()))).ToArray();
      var independentInitializers = initializers.Except(dependentInitializers).ToArray();

      var mainProcess = new SequentialProcess();
      if (independentInitializers.Length > 0)
      {
        var parallelProcess = new ParallelProcess();
        parallelProcess.AddRange(independentInitializers.Select(converter.AsSingleProcess));
        mainProcess.Add(parallelProcess);
      }

      if (dependentInitializers.Length == 0) 
        return mainProcess;

      var process = BuildSequence(dependentInitializers, converter);
      mainProcess.Add(process);

      return mainProcess;
    }
  }
}