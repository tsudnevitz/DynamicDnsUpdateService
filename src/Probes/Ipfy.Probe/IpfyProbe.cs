using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DynamicDnsUpdateService.Common.Probing;
using DynamicDnsUpdateService.Common.Probing.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Ipfy.Probe
{
  public class IpfyProbe : ProbeBase<IpfyOptions>
  {
    private readonly Timer _timer;
    private readonly HttpClient _httpClient;
    private readonly Uri _url;

    public IpfyProbe(IConfiguration configuration, ILogger logger, IProbeResultPublisher publisher)
     : base(configuration, logger, publisher)
    {
      _timer = new Timer(TimeSpan.FromSeconds(Options.QueryInterval).TotalMilliseconds);
      _timer.Elapsed += async (sender, args) => await CheckIpAsync(CancellationToken.None);
      _timer.AutoReset = true;

      _httpClient = new HttpClient();
      _url = new Uri(Options.Url);
    }
    
    public override async Task StartAsync(CancellationToken token)
    {
      if (Options.StartImmediately)
        await CheckIpAsync(token);

      _timer.Start();
    }

    public override Task StopAsync(CancellationToken token)
    {
      _timer.Stop();
      return Task.CompletedTask;
    }

    private async Task CheckIpAsync(CancellationToken token)
    {
      if (token.IsCancellationRequested)
        return;

      try
      {
        Logger.LogTrace("Checking external IP at Ipfy.");
        var ip = await _httpClient.GetStringAsync(_url);
        Logger.LogTrace($"Received IP string is: {ip}.");
        var msg = new IpAddressDiscoveredEvent(IPAddress.Parse(ip));

        if (token.IsCancellationRequested)
          return;

        await Publisher.PublishAsync(msg);
      }
      catch (Exception ex)
      {
        Logger.LogError(ex, $"Failed to check IP at Ipfy: {ex.Message}");
      }
    }
  }
}