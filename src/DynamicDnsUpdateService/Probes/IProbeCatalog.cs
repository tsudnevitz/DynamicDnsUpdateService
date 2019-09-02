using System.Collections.Generic;
using System.Threading.Tasks;

namespace DynamicDnsUpdateService.Probes
{
  public interface IProbeCatalog
  {
    IReadOnlyDictionary<string, ProbeInfo> Current { get; }
    Task RebuildCatalogAsync();
  }
}