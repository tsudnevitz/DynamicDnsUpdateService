using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DynamicDnsUpdateService.Common.Probing
{
  public abstract class ProbeBase : IProbe
  {
    protected readonly IProbeResultPublisher Publisher;
    protected readonly IConfiguration Configuration;
    protected readonly ILogger Logger;

    protected ProbeBase(IConfiguration configuration, ILogger logger, IProbeResultPublisher publisher)
    {
      Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
      Logger = logger ?? throw new ArgumentNullException(nameof(logger));
      Publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public abstract Task StartAsync(CancellationToken token);
    public abstract Task StopAsync(CancellationToken token);
  }

  public abstract class ProbeBase<T> : ProbeBase
    where T : class, IProbeOptions, new()
  {
    protected ProbeBase(IConfiguration configuration, ILogger logger, IProbeResultPublisher publisher)
      : base(configuration, logger, publisher)
    {
      Options = new T();
      Configuration.GetSection(Options.Name).Bind(Options);
    }

    protected T Options { get; }
  }
}