using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Utilities.Init;
using Core.Utilities.Init.Attributes;
using DynamicDnsUpdateService.Dns;

namespace DynamicDnsUpdateService.Initializers
{
  [OnStartRunAfter(typeof(ProbeCatalogInitializer))]
  public class ListeningInitializer : InitializerBase
  {
    private readonly IChangeDetector _detector;

    public ListeningInitializer(IChangeDetector detector)
    {
      _detector = detector ?? throw new ArgumentNullException(nameof(detector));
    }

    protected override async Task OnStartAsync(CancellationToken cancellationToken)
    {
      await _detector.StartAsync(cancellationToken);
    }

    protected override async Task OnStopAsync(CancellationToken cancellationToken)
    {
      await _detector.StopAsync(cancellationToken);
    }
  }
}
