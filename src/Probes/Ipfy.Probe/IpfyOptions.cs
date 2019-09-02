using DynamicDnsUpdateService.Common.Probing;

namespace Ipfy.Probe
{
  public class IpfyOptions : IProbeOptions
  {
    public string Name { get; } = "ipfy";
    public int QueryInterval { get; set; }
    public string Url { get; set; }
    public bool StartImmediately { get; set; }
  }
}