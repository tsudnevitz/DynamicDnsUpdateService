using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Core.Utilities.Init.Processes;
using Core.Utilities.Init.Validation;
using Microsoft.Extensions.Logging;

namespace Core.Utilities.Init
{
  public class Bootstrapper : IBootstrapper
  {
    protected readonly ILogger Logger;
    protected readonly IProcess StartupProcess;
    protected readonly IProcess ShutdownProcess;

    public TimeSpan AdditionalTime { get; set; }

    public event EventHandler<TimeSpan> AdditionalTimeRequested;

    protected Bootstrapper(ILogger logger, IEnumerable<IInitializer> initializers, IInitializersValidator validator, IProcessConstructor constructor)
    {
      Logger = logger ?? throw new ArgumentNullException(nameof(logger));
      var immutableInitializers = initializers?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(initializers));

      if (validator == null)
        throw new ArgumentNullException(nameof(validator));

      if (constructor == null)
        throw new ArgumentNullException(nameof(constructor));

      AdditionalTime = TimeSpan.FromSeconds(5);

      StartupProcess = constructor.BuildStartupProcess(immutableInitializers);
      ShutdownProcess = constructor.BuildShutdownProcess(immutableInitializers);
    }

    public async Task<bool> BeginStartupAsync(CancellationToken cancellationToken)
    {
      return await ExecuteAsync(StartupProcess, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> BeginShutdownAsync(CancellationToken cancellationToken)
    {
      return await ExecuteAsync(ShutdownProcess, cancellationToken).ConfigureAwait(false);
    }

    private async Task<bool> ExecuteAsync(IProcess process, CancellationToken cancellationToken)
    {
      if (cancellationToken.IsCancellationRequested)
      {
        Logger.Log(LogLevel.Warning, "Process execution cancelled.");
        return false;
      }

      try
      {
        AdditionalTimeRequested?.Invoke(this, AdditionalTime);

        var waitTimeoutMillis = AdditionalTime.TotalMilliseconds / 2;
        var waitTimeout = TimeSpan.FromMilliseconds(waitTimeoutMillis);
        var task = process.RunAsync(cancellationToken);

        // ReSharper disable once MethodSupportsCancellation
        while (await Task.WhenAny(task, Task.Delay(waitTimeout)) != task)
        {
          Logger.Log(LogLevel.Debug, $"Requesting additional time to finish: {AdditionalTime}.");
          AdditionalTimeRequested?.Invoke(this, AdditionalTime);
        }

        if (task.IsFaulted)
          ExceptionDispatchInfo.Capture(task.Exception).Throw();

        return true;
      }
      catch (AggregateException ex)
      {
        var flat = ex.Flatten();
        foreach (var inner in flat.InnerExceptions)
          Logger.Log(LogLevel.Critical, inner.Message, inner);

        return false;
      }
      catch (Exception ex)
      {
        Logger.Log(LogLevel.Critical, ex.Message, ex);
        return false;
      }
    }
  }
}