using System;
using Core.Utilities.Init.Hosting;
using DynamicDnsUpdateService.Common.Probing;
using DynamicDnsUpdateService.Config;
using DynamicDnsUpdateService.Dns;
using DynamicDnsUpdateService.Probes;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using IProbeFactory = DynamicDnsUpdateService.Probes.IProbeFactory;

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
        .On(Platform.Windows, x => x.UseWindowsService())
        .On(Platform.Linux, x => x.UseSystemd())
        .ConfigureAppConfiguration((context, config) =>
        {
          var env = context.HostingEnvironment;
          config.AddJsonFile("service.json", false, true);
          config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);
        })
        .ConfigureServices((context, services) =>
        {
          services.Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromMinutes(5));
          services.Configure<ExternalIpDetection>(options => context.Configuration.GetSection("external-ip-detection").Bind(options));
          services.UseBootstrapper(config => config.AddInitializersFromEntryAssembly());

          services.AddSingleton<IProbeCatalog, ProbeCatalog>();
          services.AddSingleton<IChangeDetector, ChangeDetector>();
          services.AddSingleton<IProbeFactory, ProbeFactory>();
          services.AddSingleton<IDnsProviderFactory, DnsProviderFactory>();
          services.AddSingleton<IProbeResultPublisher, ProbeResultPublisher>();
        })
        .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
          .ReadFrom.Configuration(hostingContext.Configuration)
          .Enrich.FromLogContext()
          .WriteTo.Console(theme: AnsiConsoleTheme.Literate));
  }
}
  
