using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Utilities.Init.Processes
{
  public class ParallelProcess : List<IProcess>, IComplexProcess
  {
    public ParallelProcess()
    {
    }

    private ParallelProcess(IEnumerable<IProcess> collection) 
      : base(collection)
    {
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
      if (cancellationToken.IsCancellationRequested)
        return;

      var tasks = this.Select(x => x.RunAsync(cancellationToken)).ToArray();
      await Task.WhenAll(tasks);
    }

    public IComplexProcess Create(IEnumerable<IProcess> items)
    {
      return new ParallelProcess(items);
    }
  }
}