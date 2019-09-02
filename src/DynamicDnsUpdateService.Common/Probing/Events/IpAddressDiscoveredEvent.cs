using System.Net;

namespace DynamicDnsUpdateService.Common.Probing.Events
{
  public class IpAddressDiscoveredEvent
  {
    public IpAddressDiscoveredEvent(IPAddress ipAddress)
    {
      IpAddress = ipAddress;
    }

    public IPAddress IpAddress { get; }
  }
}