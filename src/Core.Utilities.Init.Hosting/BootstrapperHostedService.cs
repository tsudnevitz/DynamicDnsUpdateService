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
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      await BeginStartupAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
      await BeginShutdownAsync(cancellationToken);
    }
  }
}
