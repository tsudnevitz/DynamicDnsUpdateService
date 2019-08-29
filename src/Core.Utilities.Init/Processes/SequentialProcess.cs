using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Utilities.Init.Processes
{
  public class SequentialProcess : List<IProcess>, IComplexProcess
  {
    public SequentialProcess()
    {
    }

    public SequentialProcess(IEnumerable<IProcess> collection) : base(collection)
    {
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
      foreach (var process in this)
      {
        if (cancellationToken.IsCancellationRequested)
          return;

        await process.RunAsync(cancellationToken);
      }
    }

    public IComplexProcess Create(IEnumerable<IProcess> items)
    {
      return new SequentialProcess(items);
    }
  }
}