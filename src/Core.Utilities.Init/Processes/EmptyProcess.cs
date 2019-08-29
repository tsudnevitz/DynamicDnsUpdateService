using System.Threading;
using System.Threading.Tasks;

namespace Core.Utilities.Init.Processes
{
  public class EmptyProcess : IProcess
  {
    private static EmptyProcess _instance;

    private EmptyProcess()
    { }

    public Task RunAsync(CancellationToken cancellationToken)
    {
      return cancellationToken.IsCancellationRequested 
        ? Task.FromCanceled(cancellationToken) 
        : Task.CompletedTask;
    }

    public static EmptyProcess Instance => _instance ??= new EmptyProcess();
  }
}