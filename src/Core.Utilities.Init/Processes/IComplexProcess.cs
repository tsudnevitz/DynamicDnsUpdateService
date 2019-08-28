using System.Collections.Generic;

namespace Core.Utilities.Init.Processes
{
  public interface IComplexProcess : IEnumerable<IProcess>, IProcess
  {
    IComplexProcess Create(IEnumerable<IProcess> items);
  }
}