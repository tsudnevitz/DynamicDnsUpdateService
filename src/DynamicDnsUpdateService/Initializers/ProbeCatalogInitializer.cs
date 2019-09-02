using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Utilities.Init;
using Core.Utilities.Init.Attributes;
using Core.Utilities.Init.Enums;
using DynamicDnsUpdateService.Probes;

namespace DynamicDnsUpdateService.Initializers
{
  [StartMode(RunModes.Beginning)]
  public class ProbeCatalogInitializer : InitializerBase
  {
    private readonly IProbeCatalog _probeCatalog;

    public ProbeCatalogInitializer(IProbeCatalog probeCatalog)
    {
      _probeCatalog = probeCatalog ?? throw new ArgumentNullException(nameof(probeCatalog));
    }

    protected override async Task OnStartAsync(CancellationToken cancellationToken)
    {
      await _probeCatalog.RebuildCatalogAsync();
    }

    protected override Task OnStopAsync(CancellationToken cancellationToken)
    {
      return Task.CompletedTask;
    }
  }
}
