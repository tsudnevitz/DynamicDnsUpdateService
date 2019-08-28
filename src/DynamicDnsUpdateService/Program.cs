using Core.Utilities.Init.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DynamicDnsUpdateService
{
  public static class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }
    
    private static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
        {
          var env = context.HostingEnvironment;
          config.AddJsonFile("service.json", false, true);
          config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);
        })
        .ConfigureServices(services =>
        {
          services.UseBootstrapper(config => config.
            AddInitializersFromEntryAssembly());
        })
        .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
          .ReadFrom.Configuration(hostingContext.Configuration)
          .Enrich.FromLogContext()
          .WriteTo.Console());
  }
}
