using System.Threading.Tasks;
using DynamicDnsUpdateService.Common.Interfaces;
using DynamicDnsUpdateService.Messeging;

namespace DynamicDnsUpdateService.Dns
{
  public interface IDnsProvider : IStartable
  {
    ValueTask UpdateDnsAsync(IpAddressChangedEvent evt);
  }
}
