using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using Core.Utilities.Init.Enums;
using Core.Utilities.Init.Processes;
using Core.Utilities.Init.Validation;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Core.Utilities.Init.Tests.Processes
{
  public class DefaultProcessBuilderTests
  {
    private readonly Fixture _fixture;
    private readonly DefaultProcessBuilder _sut;

    public DefaultProcessBuilderTests()
    {
      _fixture = new Fixture();
      _fixture.Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});

      var validator = A.Fake<IInitializersValidator>();
      A.CallTo(() => validator.Validate(A<IReadOnlyCollection<IInitializer>>._))
        .ReturnsLazily(call => new ValidationResult());

      var optimizer = A.Fake<IProcessOptimizer>();
      A.CallTo(() => optimizer.OptimizeProcess(A<IProcess>._))
        .ReturnsLazily(call => call.GetArgument<IProcess>(0));

      _fixture.Register(() => validator);
      _fixture.Register(() => optimizer);
      _sut = _fixture.Create<DefaultProcessBuilder>();
    }

    [Fact]
    public void BuildStartupProcess_NoInitializers_NoExceptionThrown()
    {
      // arrange
      var initializers = new IInitializer[0];

      // act + assert
      _sut.Invoking(x => x.BuildShutdownProcess(initializers)).Should().NotThrow();
    }

    [Theory]
    [InlineData(RunModes.Beginning)]
    [InlineData(RunModes.Ending)]
    public async Task BuildStartupProcess_MultipleSameTypeInitializers_GetsExecutedNoExceptionThrown(RunModes runMode)
    {
      // arrange
      var counter = 0;
      var invokes = new ConcurrentDictionary<int, IInitializer>();

      _fixture.Register(() =>
      {
        var initializer = A.Fake<IInitializer>();
        A.CallTo(() => initializer.StartMode).Returns(runMode);
        A.CallTo(() => initializer.StopMode).Returns(runMode);
        A.CallTo(() => initializer.StartAsync(CancellationToken.None)).ReturnsLazily(() =>
        {
          invokes.TryAdd(Interlocked.Increment(ref counter), initializer);
          return Task.CompletedTask;
        });
        return initializer;
      });

      var initializers = _fixture
        .CreateMany<IInitializer>()
        .ToImmutableArray();

      // act
      var process = _sut.BuildStartupProcess(initializers);
      await process.RunAsync(CancellationToken.None);

      // assert
      invokes.Count.Should().Be(initializers.Length);
    }

    [Theory]
    [InlineData(RunModes.First)]
    [InlineData(RunModes.Last)]
    public async Task BuildStartupProcess_SingleInitializers_GetsExecutedNoExceptionThrown(RunModes runMode)
    {
      // arrange
      var counter = 0;
      var invokes = new ConcurrentDictionary<int, IInitializer>();

      _fixture.Register(() =>
      {
        var initializer = A.Fake<IInitializer>();
        A.CallTo(() => initializer.StartMode).Returns(runMode);
        A.CallTo(() => initializer.StartAsync(CancellationToken.None)).ReturnsLazily(() =>
        {
          invokes.TryAdd(Interlocked.Increment(ref counter), initializer);
          return Task.CompletedTask;
        });
        return initializer;
      });

      var initializers = _fixture
        .CreateMany<IInitializer>(1)
        .ToImmutableArray();

      // act
      var process = _sut.BuildStartupProcess(initializers);
      await process.RunAsync(CancellationToken.None);

      // assert
      counter.Should().Be(1);
    }
  }
}