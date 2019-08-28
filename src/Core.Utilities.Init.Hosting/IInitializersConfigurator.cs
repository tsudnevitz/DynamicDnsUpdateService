using System;
using System.Collections.Generic;
using System.Reflection;

namespace Core.Utilities.Init.Hosting
{
  public interface IInitializersConfigurator
  {
    IInitializersConfigurator AddInitializer<T>() where T : IInitializer;
    IInitializersConfigurator AddInitializer(Type type);
    IInitializersConfigurator AddInitializers(Assembly assembly);
    IInitializersConfigurator AddInitializersFromEntryAssembly();
    IInitializersConfigurator AddInitializers(IEnumerable<Type> types);
  }
}