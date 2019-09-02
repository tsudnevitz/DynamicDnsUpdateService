using System;

namespace DynamicDnsUpdateService.Probes
{
  public class ProbeInfo
  {
    public ProbeInfo(string assemblyLocation, string typeFullName, string name)
    {
      AssemblyLocation = assemblyLocation ?? throw new ArgumentNullException(nameof(assemblyLocation));
      TypeFullName = typeFullName ?? throw new ArgumentNullException(nameof(typeFullName));
      Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public string AssemblyLocation { get; }
    public string TypeFullName { get; }
    public string Name { get; }

    public override string ToString()
    {
      return $"Probe name: \"{Name}\", probe type full name: \"{TypeFullName}\", assembly location: \"{AssemblyLocation}\"";
    }
  }
}