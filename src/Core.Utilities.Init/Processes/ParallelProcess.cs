using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Utilities.Init.Processes
{
  public class ParallelProcess : List<IProcess>, IComplexProcess
  {
    public ParallelProcess()
    {
    }

    public ParallelProcess(IEnumerable<IProcess> collection) 
      : base(collection)
    {
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
      if (cancellationToken.IsCancellationRequested)
        return;

      var tasks = this.Select(x => x.RunAsync(cancellationToken)).ToArray();

      // changing weird .net await aggregated exception handling
      // see: https://stackoverflow.com/questions/12007781/why-doesnt-await-on-task-whenall-throw-an-aggregateexception

      var task = Task.WhenAll(tasks);
      
      try
      {
        await task;
      }
      catch
      {
        ExceptionDispatchInfo.Capture(task.Exception).Throw();
      }
    }

    public IComplexProcess Create(IEnumerable<IProcess> items)
    {
      return new ParallelProcess(items);
    }
  }
}