using System.Threading;
using System.Threading.Tasks;

namespace DynamicDnsUpdateService.Common.Interfaces
{
  public interface IStartable
  {
    ValueTask StartAsync(CancellationToken token);
    ValueTask StopAsync(CancellationToken token);
  }
}