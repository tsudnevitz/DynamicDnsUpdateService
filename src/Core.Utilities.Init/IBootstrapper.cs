using System;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Utilities.Init
{
  public interface IBootstrapper
  {
    event EventHandler<TimeSpan> AdditionalTimeRequested;

    TimeSpan AdditionalTime { get; set; }

    Task<bool> BeginStartupAsync(CancellationToken cancellationToken);
    Task<bool> BeginShutdownAsync(CancellationToken cancellationToken);
  }
}