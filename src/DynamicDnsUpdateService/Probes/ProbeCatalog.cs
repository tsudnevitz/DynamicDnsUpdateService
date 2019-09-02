using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Core.Utilities.Init.Extensions;
using DynamicDnsUpdateService.Common.Probing;
using Microsoft.Extensions.Logging;

namespace DynamicDnsUpdateService.Probes
{
  public class ProbeCatalog : IProbeCatalog
  {
    private readonly ILogger<ProbeCatalog> _logger;
    private readonly DirectoryInfo _probesDirectory;

    public ProbeCatalog(ILogger<ProbeCatalog> logger)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      Current = new Dictionary<string, ProbeInfo>(0);

      var probesPath = GetProbesPath();
      _probesDirectory = new DirectoryInfo(probesPath);

      if (!_probesDirectory.Exists)
        throw new DirectoryNotFoundException("Probes directory not found.");
    }

    public IReadOnlyDictionary<string, ProbeInfo> Current { get; private set; }

    private string GetProbesPath()
    {
      var path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ?? Directory.GetCurrentDirectory());
      _logger.LogDebug($"Probes path set to: {path}");
      return path;
    }

    public async Task RebuildCatalogAsync()
    {
      var newCatalog = new Dictionary<string, ProbeInfo>();
      var files = _probesDirectory.GetFiles("*.Probe.dll");
      _logger.LogTrace($"Found {files.Length} potential probe assemblies.");

      using var context = new SideLoadContext(_logger);
      foreach (var file in files)
      {
        try
        {
          _logger.LogTrace($"Examining assembly: {file.Name}");
          await using var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
          var assembly = context.LoadFromStream(stream);

          var probeInfos = GetProbeInfos(file.FullName, assembly);
          _logger.Log(
            probeInfos.Length == 0 ? LogLevel.Warning : LogLevel.Information,
            $"Discovered {probeInfos.Length} probe info(s) in assembly: {file.Name}.");

          foreach (var probeInfo in probeInfos)
            newCatalog.Add(probeInfo.Name, probeInfo);

          Current = newCatalog;
        }
        catch (Exception ex)
        {
          _logger.LogWarning(ex, $"Unable to load assembly at path: {file.FullName}");
        }
      }
    }

    private ProbeInfo[] GetProbeInfos(string assemblyLocation, Assembly assembly)
    {
      var probeInfos = new List<ProbeInfo>();
      var probeTypes = assembly.ExportedTypes.Where(x => x.IsAssignableToGenericType(typeof(ProbeBase<>)));
      
      foreach (var probeType in probeTypes)
      {
        try
        {
          var baseType = GetProbeBaseType(probeType.BaseType);
          var optionsType = baseType.GetGenericArguments()[0];
          var options = (IProbeOptions) assembly.CreateInstance(optionsType.FullName);
          var probeInfo = new ProbeInfo(assemblyLocation, probeType.FullName, options.Name);

          _logger.LogDebug($"Found probe: {probeInfo}.");
          probeInfos.Add(probeInfo);
        }
        catch (Exception ex)
        {
          _logger.LogWarning(ex, $"Failed to load: {probeType}");
        }
      }

      return probeInfos.ToArray();
    }

    private static Type GetProbeBaseType(Type baseType)
    {
      if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(ProbeBase<>))
        return baseType;

      if (baseType == null)
        throw new InvalidOperationException($"Supplied type does not inherit {typeof(ProbeBase<>).Name} type.");

      return GetProbeBaseType(baseType.BaseType);
    }
  }
}