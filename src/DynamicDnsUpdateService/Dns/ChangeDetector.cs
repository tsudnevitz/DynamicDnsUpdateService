using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DynamicDnsUpdateService.Common.Probing;
using DynamicDnsUpdateService.Common.Probing.Events;
using DynamicDnsUpdateService.Config;
using DynamicDnsUpdateService.Messeging;
using DynamicDnsUpdateService.Probes;
using Microsoft.Extensions.Options;

namespace DynamicDnsUpdateService.Dns
{
  public class ChangeDetector : IChangeDetector
  {
    private readonly Lazy<IProbe> _probe;
    private readonly Lazy<IDnsProvider> _provider;

    private IPAddress _currentIpAddress = IPAddress.None;

    public ChangeDetector(IOptions<ExternalIpDetection> options, IProbeFactory probeFactory, IDnsProviderFactory providerFactory)
    {
      if (probeFactory == null)
        throw new ArgumentNullException(nameof(probeFactory));

      if (providerFactory == null)
        throw new ArgumentNullException(nameof(providerFactory));

      _probe = new Lazy<IProbe>(() => probeFactory.Create(options.Value.Mode));
      _provider = new Lazy<IDnsProvider>(providerFactory.Create);
    }

    public async ValueTask StartAsync(CancellationToken token)
    {
      if (token.IsCancellationRequested)
        return;

      await _probe.Value.StartAsync(token);
      //await _provider.Value.StartAsync(token);
    }

    public async ValueTask HandleAsync(IpAddressDiscoveredEvent evt)
    {
      var oldValue = Interlocked.CompareExchange(ref _currentIpAddress, evt.IpAddress, evt.IpAddress);
      if (Equals(_currentIpAddress, oldValue))
        return;

      var message = new IpAddressChangedEvent(oldValue, evt.IpAddress);
      await _provider.Value.UpdateDnsAsync(message);
    }

    public async ValueTask StopAsync(CancellationToken token)
    {
      if (token.IsCancellationRequested)
        return;

      if (_probe.IsValueCreated)
        await _probe.Value.StopAsync(token);

      if (_provider.IsValueCreated)
        await _provider.Value.StopAsync(token);
    }
  }
}