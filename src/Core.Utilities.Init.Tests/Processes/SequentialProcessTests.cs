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
  public class SequentialProcessTests
  {
    private readonly Fixture _fixture;

    public SequentialProcessTests()
    {
      _fixture = new Fixture();
      _fixture.Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
    }

    [Fact]
    public void RunAsync_NoProcessesToRun_NoExceptions()
    {
      // arrange
      var sut = new SequentialProcess();
      
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
      var sut = new SequentialProcess(processes);

      // act
      await sut.RunAsync(CancellationToken.None);

      // assert
      dict.Should().ContainKeys(expectedKeys);
    }    
    
    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    public async Task RunAsync_ManyProcesses_RunInSequence(int processCount)
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
      var sut = new SequentialProcess(processes);

      // act
      await sut.RunAsync(CancellationToken.None);

      // assert
      maxProcessesRunningInParallel.Should().Be(1);
      runOrder.Should().HaveCount(processCount);
      runOrder.Should().BeInAscendingOrder();
    }

    [Fact]
    public void RunAsync_TaskThrows_Throws()
    {
      // arrange
      var expectedException = new Exception("I'm failing You, I'm so sorry.");
      var process = A.Fake<IProcess>();
      A.CallTo(() => process.RunAsync(CancellationToken.None)).Returns(Task.FromException(expectedException));

      var sut = new SequentialProcess(new[] {process});

      // act + assert
      sut.Invoking(async x => await x.RunAsync(CancellationToken.None))
        .Should().Throw<Exception>().Where(ex => ex == expectedException);
    }

    [Fact]
    public void RunAsync_FirstTaskThrows_SecondTaksNewerInvoked()
    {
      // arrange
      var expectedException = new Exception("I'm failing You, I'm so sorry.");
      var firstProcess = A.Fake<IProcess>();
      A.CallTo(() => firstProcess.RunAsync(CancellationToken.None)).Returns(Task.FromException(expectedException));

      var secondProcessWasInvoked = false;
      var secondProcess = A.Fake<IProcess>();
      A.CallTo(() => secondProcess.RunAsync(CancellationToken.None)).Invokes(() => secondProcessWasInvoked = true);

      var sut = new SequentialProcess(new[] {firstProcess, secondProcess});

      // act
      sut.Invoking(async x => await x.RunAsync(CancellationToken.None))
        .Should().Throw<Exception>().Where(ex => ex == expectedException);
      
      // assert
      secondProcessWasInvoked.Should().BeFalse();
    }

    [Fact]
    public void Create_ListOfProcesses_ShouldContainSameProcesses()
    {
      // arrange
      var subProcesses = _fixture.CreateMany<IProcess>().ToArray();
      var sut = new SequentialProcess();

      // act
      var process = sut.Create(subProcesses);

      // assert
      process.Should().BeEquivalentTo(subProcesses);
    }
  }
}