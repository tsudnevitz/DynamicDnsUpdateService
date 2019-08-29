using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Utilities.Init;
using Core.Utilities.Init.Attributes;
using Core.Utilities.Init.Enums;
using Microsoft.Extensions.Logging;

namespace DynamicDnsUpdateService.Initializers
{
  [StartMode(RunModes.Beginning)]
  [StopMode(RunModes.Ending)]
  [OnStartRunAfter(typeof(TestInitializer2))]
  public class TestInitializer : InitializerBase
  {
    private readonly ILogger _logger;

    public TestInitializer(ILogger<TestInitializer> logger)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task OnStartAsync(CancellationToken cancellationToken)
    {
      _logger.Log(LogLevel.Information, $"Starting {GetType().Name}.");
      await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
      _logger.Log(LogLevel.Information, $"Started {GetType().Name}.");

    }

    protected override async Task OnStopAsync(CancellationToken cancellationToken)
    {
      _logger.Log(LogLevel.Information, $"Stopping {GetType().Name}.");
      await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
      _logger.Log(LogLevel.Information, $"Stopped {GetType().Name}.");
    }
  }

  [StartMode(RunModes.Beginning)]
  [StopMode(RunModes.Ending)]
  public class TestInitializer2 : InitializerBase
  {
    private readonly ILogger _logger;

    public TestInitializer2(ILogger<TestInitializer2> logger)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task OnStartAsync(CancellationToken cancellationToken)
    {
      _logger.Log(LogLevel.Information, $"Starting {GetType().Name}.");
      await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
      _logger.Log(LogLevel.Information, $"Started {GetType().Name}.");

    }

    protected override async Task OnStopAsync(CancellationToken cancellationToken)
    {
      _logger.Log(LogLevel.Information, $"Stopping {GetType().Name}.");
      await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
      _logger.Log(LogLevel.Information, $"Stopped {GetType().Name}.");
    }
  }
}
