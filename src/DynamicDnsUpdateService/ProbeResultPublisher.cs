using System.Threading.Tasks;
using DynamicDnsUpdateService.Common.Probing;
using DynamicDnsUpdateService.Common.Probing.Events;

namespace DynamicDnsUpdateService
{
  public class ProbeResultPublisher : IProbeResultPublisher
  {
    public async Task PublishAsync(IpAddressDiscoveredEvent evt)
    {
      await Task.CompletedTask;
    }
  }
}