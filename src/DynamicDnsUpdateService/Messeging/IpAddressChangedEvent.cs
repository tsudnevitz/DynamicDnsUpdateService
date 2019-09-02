using System.Net;

namespace DynamicDnsUpdateService.Messeging
{
  public class IpAddressChangedEvent
  {
    public IpAddressChangedEvent(IPAddress oldIpAddress, IPAddress newIpAddress)
    {
      NewIpAddress = newIpAddress;
      OldIpAddress = oldIpAddress;
    }

    public IPAddress NewIpAddress { get; }
    public IPAddress OldIpAddress { get; }
  }
}