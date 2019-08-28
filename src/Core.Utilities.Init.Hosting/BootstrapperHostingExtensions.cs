using System;
using System.Collections.Generic;
using Core.Utilities.Init.Processes;
using Core.Utilities.Init.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Utilities.Init.Hosting
{
  public static class BootstrapperHostingExtensions
  {
    public static IServiceCollection UseBootstrapper(
      this IServiceCollection services,
      Action<IInitializersConfigurator> config,
      IInitializersValidator validator = null,
      IProcessConstructor constructor = null)
    {
      if (config == null)
        throw new ArgumentNullException(nameof(config));

      var init = new InitializerConfigurator();
      config(init);

      return UseBootstrapperInternal(services, init, validator, constructor);
    }

    private static IServiceCollection UseBootstrapperInternal(
      IServiceCollection services, 
      IEnumerable<Type> initializers,
      IInitializersValidator validator = null, 
      IProcessConstructor constructor = null)
    {
      if (initializers == null)
        throw new ArgumentNullException(nameof(initializers));
      
      foreach (var type in initializers)
        services.AddSingleton(typeof(IInitializer), type);

      if (services == null)
        throw new ArgumentNullException(nameof(services));

      if (validator == null)
        services.AddSingleton(typeof(IInitializersValidator), typeof(DefaultInitializersValidator));
      else
        services.AddSingleton(validator);

      if (constructor == null)
      {
        services.AddSingleton(typeof(IProcessConstructor), typeof(DefaultProcessConstructor));
        services.AddSingleton(typeof(IProcessOptimizer), typeof(DefaultProcessOptimizer));
      }
      else
        services.AddSingleton(constructor);

      return services.AddHostedService<BootstrapperHostedService>();
    }
  }
}
