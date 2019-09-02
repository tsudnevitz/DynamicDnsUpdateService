using System.Threading.Tasks;
using DynamicDnsUpdateService.Common.Probing.Events;

namespace DynamicDnsUpdateService.Common.Probing
{
  public interface IProbeResultPublisher
  {
    Task PublishAsync(IpAddressDiscoveredEvent evt);
  }
}