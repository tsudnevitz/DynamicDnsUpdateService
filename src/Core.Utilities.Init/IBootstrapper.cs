using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Utilities.Init.Enums;

namespace Core.Utilities.Init
{
  public interface IBootstrapper
  {
    event EventHandler<TimeSpan> AdditionalTimeRequested;
    event EventHandler<BootstrapperStates> StateChanged;

    TimeSpan AdditionalTime { get; set; }

    BootstrapperStates State { get; }

    Task<bool> BeginStartupAsync(CancellationToken cancellationToken);
    Task<bool> BeginShutdownAsync(CancellationToken cancellationToken);
  }
}