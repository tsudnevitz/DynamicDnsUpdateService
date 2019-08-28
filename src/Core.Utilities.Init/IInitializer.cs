using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Utilities.Init.Enums;

namespace Core.Utilities.Init
{
  public interface IInitializer
  {
    RunModes StartMode { get; }
    RunModes StopMode { get; }
    Type[] OnStartRunAfter { get; }
    Type[] OnStopRunAfter { get; }

    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
  }
}
