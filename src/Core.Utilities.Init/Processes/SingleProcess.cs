using System;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Utilities.Init.Processes
{
  public class SingleProcess : IProcess
  {
    private readonly Func<CancellationToken, Task> _action;

    public SingleProcess(Func<CancellationToken, Task> action)
    {
      _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
      if (cancellationToken.IsCancellationRequested)
        return;

      await _action(cancellationToken);
    }
  }
}