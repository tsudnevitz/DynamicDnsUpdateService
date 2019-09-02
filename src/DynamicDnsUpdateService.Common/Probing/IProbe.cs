using System.Threading;
using System.Threading.Tasks;

namespace DynamicDnsUpdateService.Common.Probing
{
  public interface IProbe
  {
    Task StartAsync(CancellationToken token);
    Task StopAsync(CancellationToken token);
  }
}