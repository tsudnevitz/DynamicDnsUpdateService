using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Utilities.Init.Processes;
using Core.Utilities.Init.Validation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Core.Utilities.Init.Hosting
{
  public class BootstrapperHostedService : Bootstrapper, IHostedService
  {
    public BootstrapperHostedService(IEnumerable<IInitializer> initializers, ILogger<BootstrapperHostedService> logger, IInitializersValidator validator, IProcessConstructor constructor) 
      : base(logger, initializers, validator, constructor)
    {
      StateChanged += (_, states) => logger.LogInformation($"Bootstrapper state: {states}.");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      var result = await BeginStartupAsync(cancellationToken);
      
      if (!result)
        throw new ApplicationException("Application did not start clearly. Examine logs.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
      var result = await BeginShutdownAsync(cancellationToken);

      if (!result)
        throw new ApplicationException("Application did not stop clearly. Examine logs.");
    }
  }
}
