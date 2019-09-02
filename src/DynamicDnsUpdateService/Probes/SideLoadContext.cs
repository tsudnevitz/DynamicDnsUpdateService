using System;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;

namespace DynamicDnsUpdateService.Probes
{
  internal class SideLoadContext : AssemblyLoadContext, IDisposable
  {
    private readonly ILogger _logger;

    public SideLoadContext(ILogger logger)
     : base($"{nameof(SideLoadContext)}_{Guid.NewGuid():N}", isCollectible:true)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      try
      {
        Unload();
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "Load context failed to unload.");
      }
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~SideLoadContext()
    {
      Dispose(false);
    }
  }
}