using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using DynamicDnsUpdateService.Common.Probing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DynamicDnsUpdateService.Probes
{
  public class ProbeFactory : IProbeFactory
  {
    private readonly IProbeCatalog _catalog;
    private readonly Dictionary<string, ProbeContext> _probes = new Dictionary<string, ProbeContext>();
    private readonly object _lock = new object();
    private readonly ILogger<ProbeFactory> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IConfiguration _configuration;
    private readonly IProbeResultPublisher _publisher;

    public ProbeFactory(IProbeCatalog catalog, ILogger<ProbeFactory> logger, ILoggerFactory loggerFactory, IConfiguration configuration, IProbeResultPublisher publisher)
    {
      _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
      _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
      _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public IProbe Create(string probeName)
    {
      if (probeName == null)
        throw new ArgumentNullException(nameof(probeName));
      
      _logger.LogTrace($"Creating probe: {probeName}");
      if (!_catalog.Current.TryGetValue(probeName, out var probeInfo))
        throw new ArgumentOutOfRangeException(nameof(probeName), $"Probe named {probeName} not found.");

      lock (_lock)
      {
        if (_probes.TryGetValue(probeName, out var context))
          return context.Probe;

        _logger.LogTrace($"Probe not yet loaded, side loading: {probeInfo}");
        var probeLogger = _loggerFactory.CreateLogger(probeInfo.TypeFullName);
        var loader = new SideLoadContext(_loggerFactory.CreateLogger<SideLoadContext>());
        var assembly = loader.LoadFromAssemblyPath(probeInfo.AssemblyLocation);
        var probe = (IProbe) assembly.CreateInstance(probeInfo.TypeFullName, false, BindingFlags.Instance | BindingFlags.Public, null, new object[]{_configuration, probeLogger, _publisher}, CultureInfo.CurrentCulture, null);

        context = new ProbeContext(loader, probe);
        _probes.Add(probeInfo.Name, context);

        return context.Probe;
      }
    }
  }
}