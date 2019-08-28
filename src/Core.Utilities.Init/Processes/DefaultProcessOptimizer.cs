using System.Collections.Generic;
using System.Linq;

namespace Core.Utilities.Init.Processes
{
  public class DefaultProcessOptimizer : IProcessOptimizer
  {
    public IProcess OptimizeProcess(IProcess process)
    {
      if (!(process is IComplexProcess processes))
        return process;

      var processesInside = processes.Count();
      if (processesInside == 1)
      {
        var firstProcess = processes.First();
        if (!(firstProcess is IComplexProcess complexProcess)) 
          return firstProcess;

        return OptimizeProcess(complexProcess);
      }
      
      var optimizedProcesses = new List<IProcess>();
      foreach (var innerProcess in processes)
      {
        if (innerProcess is IComplexProcess complexProcess)
        {
          var optimizedProcess = OptimizeProcess(complexProcess);
          optimizedProcesses.Add(optimizedProcess);
        }
        else
        {
          optimizedProcesses.Add(innerProcess);
        }
      }

      return processes.Create(optimizedProcesses);
    }
  }
}