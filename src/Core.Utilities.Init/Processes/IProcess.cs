using System.Threading;
using System.Threading.Tasks;

namespace Core.Utilities.Init.Processes
{
  public interface IProcess
  {
    Task RunAsync(CancellationToken cancellationToken);
  }
}