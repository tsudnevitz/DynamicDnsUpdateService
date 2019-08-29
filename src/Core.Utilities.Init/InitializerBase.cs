using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Utilities.Extensions;
using Core.Utilities.Init.Attributes;
using Core.Utilities.Init.Enums;

namespace Core.Utilities.Init
{
  public abstract class InitializerBase : IInitializer
  {
    private readonly object _lock = new object();

    private bool _isInitialized;
    private bool _isInitializing;
    private bool _isStopping;
    
    protected InitializerBase()
    {
      var startModeAttribute = this.GetAttribute<StartModeAttribute>();
      var stopModeAttribute = this.GetAttribute<StopModeAttribute>();
      var onStartRunAfterAttributes = this.GetAttributes<OnStartRunAfterAttribute>();
      var onStopRunAfterAttributes = this.GetAttributes<OnStopRunAfterAttribute>();

      StartMode = startModeAttribute?.Mode ?? RunModes.Beginning;
      StopMode = stopModeAttribute?.Mode ?? RunModes.Ending;
      OnStartRunAfter = onStartRunAfterAttributes?.Select(x => x.Type).ToArray() ?? new Type[0];
      OnStopRunAfter = onStopRunAfterAttributes?.Select(x => x.Type).ToArray() ?? new Type[0];
    }

    public RunModes StartMode { get; }
    public RunModes StopMode { get; }
    public Type[] OnStartRunAfter { get; }
    public Type[] OnStopRunAfter { get; }

    protected abstract Task OnStartAsync(CancellationToken cancellationToken);

    protected abstract Task OnStopAsync(CancellationToken cancellationToken);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      if (cancellationToken == null)
        throw new ArgumentNullException(nameof(cancellationToken));

      ThrowOnInvalidStartState();

      lock (_lock)
      {
        ThrowOnInvalidStartState();
        _isInitializing = true;
      }

      try
      {
        await OnStartAsync(cancellationToken);
        _isInitialized = true;
      }
      finally
      {
        _isInitializing = false;
      }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
      if (cancellationToken == null)
        throw new ArgumentNullException(nameof(cancellationToken));

      ThrowOnInvalidStopState();

      lock (_lock)
      {
        ThrowOnInvalidStopState();
        _isStopping = true;
      }
      
      try
      {
        await OnStopAsync(cancellationToken);
        _isInitialized = false;
      }
      finally
      {
        _isStopping = false;
      }
    }

    private void ThrowOnInvalidStartState()
    {
      if (_isInitializing)
        throw new InvalidOperationException("Already starting.");

      if (_isInitialized)
        throw new InvalidOperationException("Already started.");
    }

    private void ThrowOnInvalidStopState()
    {
      if (!_isInitialized && !_isInitializing)
        throw new InvalidOperationException("Not started.");

      if (_isStopping)
        throw new InvalidOperationException("Already stopping.");
    }
  }
}
