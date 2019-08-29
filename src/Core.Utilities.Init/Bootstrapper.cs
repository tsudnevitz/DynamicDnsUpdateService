using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Core.Utilities.Init.Enums;
using Core.Utilities.Init.Processes;
using Core.Utilities.Init.Validation;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;

namespace Core.Utilities.Init
{
  public class Bootstrapper : IBootstrapper
  {
    protected readonly ILogger Logger;
    protected readonly Lazy<IProcess> StartProcess;
    protected readonly Lazy<IProcess> StopProcess;

    private readonly AsyncReaderWriterLock _lock = new AsyncReaderWriterLock();
    private BootstrapperStates _state = BootstrapperStates.Stopped;

    public event EventHandler<TimeSpan> AdditionalTimeRequested;
    public event EventHandler<BootstrapperStates> StateChanged;

    public Bootstrapper(ILogger<Bootstrapper> logger, IEnumerable<IInitializer> initializers, IInitializersValidator validator, IProcessConstructor builder)
    {
      Logger = logger ?? throw new ArgumentNullException(nameof(logger));
      var immutableInitializers = initializers?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(initializers));

      if (validator == null)
        throw new ArgumentNullException(nameof(validator));

      if (builder == null)
        throw new ArgumentNullException(nameof(builder));

      var result = validator.Validate(immutableInitializers);
      if (!result.IsValid)
        throw new ValidationException(result);

      AdditionalTime = TimeSpan.FromSeconds(5);

      StartProcess = new Lazy<IProcess>(() => builder.BuildStartupProcess(immutableInitializers));
      StopProcess = new Lazy<IProcess>(() => builder.BuildShutdownProcess(immutableInitializers));
    }

    public TimeSpan AdditionalTime { get; set; }

    public BootstrapperStates State
    {
      get => _state;
      private set
      {
        if (_state == value)
          return;

        _state = value;
        StateChanged?.Invoke(this, value);
      }
    }

    public async Task<bool> BeginStartupAsync(CancellationToken cancellationToken)
    {
      return await ExecuteTransitionAsync(
        BootstrapperStates.Stopped,
        BootstrapperStates.Starting,
        BootstrapperStates.Started,
        StartProcess.Value,
        cancellationToken);
    }

    public async Task<bool> BeginShutdownAsync(CancellationToken cancellationToken)
    {
      return await ExecuteTransitionAsync(
        BootstrapperStates.Started,
        BootstrapperStates.Stopping,
        BootstrapperStates.Stopped,
        StopProcess.Value,
        cancellationToken);
    }

    private async Task<bool> ExecuteTransitionAsync(
      BootstrapperStates initialState,
      BootstrapperStates transientState,
      BootstrapperStates finalState,
      IProcess process,
      CancellationToken cancellationToken)
    {
      if (_state != initialState)
        throw new InvalidOperationException($"Invalid bootstrapper state. Expected: {initialState}, found: {_state}.");

      using (_lock.WriterLock(cancellationToken))
      {
        State = transientState;
        var result = await ExecuteProcessAsync(process, cancellationToken);
        State = result ? finalState : BootstrapperStates.Faulted;
        return result;
      }
    }

    private async Task<bool> ExecuteProcessAsync(IProcess process, CancellationToken cancellationToken)
    {
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