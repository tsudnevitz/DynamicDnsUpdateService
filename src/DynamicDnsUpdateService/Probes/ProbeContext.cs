using System;
using DynamicDnsUpdateService.Common.Probing;

namespace DynamicDnsUpdateService.Probes
{
  internal class ProbeContext : IDisposable
  {
    public ProbeContext(SideLoadContext context, IProbe probe)
    {
      Context = context ?? throw new ArgumentNullException(nameof(context));
      Probe = probe ?? throw new ArgumentNullException(nameof(probe));
    }

    public SideLoadContext Context { get; private set; }
    public IProbe Probe { get; private set; }

    public void Dispose()
    {
      Probe = null;
      Context?.Dispose();
      Context = null;
    }
  }
}