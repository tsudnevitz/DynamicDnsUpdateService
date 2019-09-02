using System;
using System.Collections.Generic;
using System.Reflection;
using Core.Utilities.Init.Extensions;

namespace Core.Utilities.Init.Hosting
{
  internal class InitializerConfigurator : HashSet<Type>, IInitializersConfigurator
  {
    public IInitializersConfigurator AddInitializer<T>() where T : IInitializer
    {
      return AddInitializer(typeof(T));
    }

    public IInitializersConfigurator AddInitializer(Type type)
    {
      if (type == null)
        throw new ArgumentNullException(nameof(type));

      if (!typeof(IInitializer).IsAssignableFrom(type))
        throw new ArgumentOutOfRangeException(nameof(type), type, $"Expected type implementing {nameof(IInitializer)}.");

      Add(type);
      return this;
    }

    public IInitializersConfigurator AddInitializersFromEntryAssembly()
    {
      return AddInitializers(Assembly.GetEntryAssembly());
    }

    public IInitializersConfigurator AddInitializers(Assembly assembly)
    {
      if (assembly == null)
        throw new ArgumentNullException(nameof(assembly));

      var types = assembly.GetTypesImplementing<IInitializer>();
      return AddInitializers(types);
    }

    public IInitializersConfigurator AddInitializers(IEnumerable<Type> types)
    {
      if (types == null)
        throw new ArgumentNullException(nameof(types));

      foreach (var type in types)
        AddInitializer(type);

      return this;
    }
  }
}