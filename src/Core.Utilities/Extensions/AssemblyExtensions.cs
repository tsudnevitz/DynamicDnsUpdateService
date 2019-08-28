using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Core.Utilities.Extensions
{
  public static class AssemblyExtensions
  {
    public static IEnumerable<Type> GetTypesImplementing<T>(this Assembly assembly)
    {
      return GetTypesImplementing(assembly, typeof(T));
    }

    public static IEnumerable<Type> GetTypesImplementing(this Assembly assembly, Type type)
    {
      if (assembly == null)
        throw new ArgumentNullException(nameof(assembly));

      if (type == null)
        throw new ArgumentNullException(nameof(type));

      return assembly.ExportedTypes.Where(x => 
          x.IsClass && 
          !x.IsAbstract && 
          type.IsAssignableFrom(x))
        .ToArray();
    }
  }
}