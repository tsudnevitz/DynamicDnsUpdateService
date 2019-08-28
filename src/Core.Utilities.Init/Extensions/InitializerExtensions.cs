using System;
using System.Threading;
using Core.Utilities.Init.Processes;

namespace Core.Utilities.Init.Extensions
{
  public static class InitializerExtensions
  {
    public static SingleProcess AsStartingProcess(this IInitializer initializer)
    {
      if (initializer == null)
        throw new ArgumentNullException(nameof(initializer));

      return new SingleProcess(initializer.StartAsync);
    }

    public static SingleProcess AsStoppingProcess(this IInitializer initializer)
    {
      if (initializer == null)
        throw new ArgumentNullException(nameof(initializer));
      
      return new SingleProcess(initializer.StopAsync);
    }
  }
}