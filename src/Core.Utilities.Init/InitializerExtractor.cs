using System;
using Core.Utilities.Init.Enums;
using Core.Utilities.Init.Extensions;
using Core.Utilities.Init.Processes;

namespace Core.Utilities.Init
{
  public class InitializerExtractor
  {
    private static readonly Func<IInitializer, RunModes, bool> StartModeSelector =
      (initializer, mode) => initializer.StartMode == mode;

    private static readonly Func<IInitializer, RunModes, bool> StopModeSelector =
      (initializer, mode) => initializer.StopMode == mode;

    private static readonly Func<IInitializer, SingleProcess> StartModeConverter =
      initializer => initializer.AsStartingProcess();

    private static readonly Func<IInitializer, SingleProcess> StopModeConverter =
      initializer => initializer.AsStoppingProcess();

    private static readonly Func<IInitializer, Type[]> StartModeDependencies =
      initializer => initializer.OnStartRunAfter;

    private static readonly Func<IInitializer, Type[]> StopModeDependencies = initializer => initializer.OnStopRunAfter;

    private readonly Func<IInitializer, RunModes, bool> _selector;
    private readonly Func<IInitializer, SingleProcess> _converter;
    private readonly Func<IInitializer, Type[]> _dependencies;

    private InitializerExtractor(
      Func<IInitializer, RunModes, bool> selector,
      Func<IInitializer, SingleProcess> converter,
      Func<IInitializer, Type[]> dependencies)
    {
      _selector = selector;
      _converter = converter;
      _dependencies = dependencies;
    }

    public bool WhereModeEquals(IInitializer initializer, RunModes mode)
    {
      return _selector(initializer, mode);
    }

    public SingleProcess AsSingleProcess(IInitializer initializer)
    {
      return _converter(initializer);
    }

    public Type[] GetDependencies(IInitializer initializer)
    {
      return _dependencies(initializer);
    }

    public static InitializerExtractor ForStartup()
    {
      return new InitializerExtractor(StartModeSelector, StartModeConverter, StartModeDependencies);
    }

    public static InitializerExtractor ForShutdown()
    {
      return new InitializerExtractor(StopModeSelector, StopModeConverter, StopModeDependencies);
    }
  }
}