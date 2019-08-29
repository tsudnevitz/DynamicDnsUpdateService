using System;
using Core.Utilities.Init.Hosting;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

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
        .UseWindowsService()
        .On(Platform.Windows, x => x.UseWindowsService())
        .On(Platform.Linux, x => x.UseSystemd())
        .ConfigureAppConfiguration((context, config) =>
        {
          var env = context.HostingEnvironment;
          config.AddJsonFile("service.json", false, true);
          config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);
        })
        .ConfigureServices(services =>
        {
          services.Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromMinutes(5));
          services.UseBootstrapper(config => config.AddInitializersFromEntryAssembly());
        })
        .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
          .ReadFrom.Configuration(hostingContext.Configuration)
          .Enrich.FromLogContext()
          .WriteTo.Console(theme: AnsiConsoleTheme.Literate));
  }
}
