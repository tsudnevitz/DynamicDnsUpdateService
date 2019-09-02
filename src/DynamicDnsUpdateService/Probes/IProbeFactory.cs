using DynamicDnsUpdateService.Common.Probing;

namespace DynamicDnsUpdateService.Probes
{
  public interface IProbeFactory
  {
    IProbe Create(string probeName);
  }
}