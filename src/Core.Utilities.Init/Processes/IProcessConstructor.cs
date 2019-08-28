using System.Collections.Generic;

namespace Core.Utilities.Init.Processes
{
  public interface IProcessConstructor
  {
    IProcess BuildStartupProcess(IReadOnlyCollection<IInitializer> initializers);
    IProcess BuildShutdownProcess(IReadOnlyCollection<IInitializer> initializers);
  }
}