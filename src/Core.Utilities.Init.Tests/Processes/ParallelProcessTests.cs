using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using Core.Utilities.Init.Processes;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Core.Utilities.Init.Tests.Processes
{
  public class ParallelProcessTests
  {
    private readonly Fixture _fixture;

    public ParallelProcessTests()
    {
      _fixture = new Fixture();
      _fixture.Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
    }

    [Fact]
    public void RunAsync_NoProcessesToRun_NoExceptions()
    {
      // arrange
      var sut = new ParallelProcess();
      
      // act + assert
      sut.Invoking(async process => await process.RunAsync(CancellationToken.None))
        .Should().NotThrow();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public async Task RunAsync_SubProcessListSupplied_AllAreRun(int processCount)
    {
      // arrange
      var processId = 0;
      var expectedKeys = Enumerable.Range(1, processCount);
      var dict = new ConcurrentDictionary<int, DateTime>();

      _fixture.Register(() =>
      {
        var process = A.Fake<IProcess>();
        A.CallTo(() => process.RunAsync(CancellationToken.None)).ReturnsLazily(() =>
        {
          var id = Interlocked.Increment(ref processId);
          dict.TryAdd(id, DateTime.Now);
          return Task.CompletedTask;
        });
        return process;
      });

      var processes = _fixture.CreateMany<IProcess>(processCount);
      var sut = new ParallelProcess(processes);

      // act
      await sut.RunAsync(CancellationToken.None);

      // assert
      dict.Should().ContainKeys(expectedKeys);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void RunAsync_ManyProcessesThrows_ThrowsAggregateExceptionForProcesses(int processCount)
    {
      // arrange
      var counter = 0;
      
      _fixture.Register(() =>
      {
        var processId = Interlocked.Increment(ref counter);
        var expectedException = new Exception($"I'm failing You, I'm so sorry. Process id: {processId}.");

        var process = A.Fake<IProcess>();
        A.CallTo(() => process.RunAsync(CancellationToken.None)).Returns(Task.FromException(expectedException));
        return process;
      });

      var processes = _fixture.CreateMany<IProcess>(processCount);
      var sut = new ParallelProcess(processes);

      // act + assert
      sut.Invoking(async x => await x.RunAsync(CancellationToken.None))
        .Should().Throw<AggregateException>().Where(ex => ex.InnerExceptions.Count == processCount);
    }
    
    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task RunAsync_ManyProcesses_RunInParallel(int processCount)
    {
      // arrange
      var processIdCounter = 0;
      var runOrder = new BlockingCollection<int>(processCount);
      var parallelProcessCounter = 0;
      var maxProcessesRunningInParallel = 0;
      var randomizer = new Random(Environment.TickCount);

      _fixture.Register(() =>
      {
        var process = A.Fake<IProcess>();
        var processId = Interlocked.Increment(ref processIdCounter);

        A.CallTo(() => process.RunAsync(CancellationToken.None)).ReturnsLazily(async () =>
        {
          var runningProcesses = Interlocked.Increment(ref parallelProcessCounter);

          maxProcessesRunningInParallel = Math.Max(runningProcesses, maxProcessesRunningInParallel);
          await Task.Delay(randomizer.Next(minValue:1, maxValue:100));
          
          runOrder.Add(processId);
          Interlocked.Decrement(ref parallelProcessCounter);
        });

        return process;
      });

      var processes = _fixture.CreateMany<IProcess>(processCount);
      var sut = new ParallelProcess(processes);

      // act
      await sut.RunAsync(CancellationToken.None);

      // assert
      maxProcessesRunningInParallel.Should().BeGreaterOrEqualTo(2);
      runOrder.Should().HaveCount(processCount);
      runOrder.Should().NotBeAscendingInOrder();
    }

    [Fact]
    public void Create_ListOfProcesses_ShouldContainSameProcesses()
    {
      // arrange
      var subProcesses = _fixture.CreateMany<IProcess>().ToArray();
      var sut = new ParallelProcess();

      // act
      var process = sut.Create(subProcesses);

      // assert
      process.Should().BeEquivalentTo(subProcesses);
    }
  }
}