using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Utilities.Init.Processes;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Core.Utilities.Init.Tests.Processes
{
  public class SingleProcessTests
  {
    [Fact]
    public async Task RunAsync_StartsTask()
    {
      // arrange
      var func = A.Fake<Func<CancellationToken, Task>>();
      var sut = new SingleProcess(func);

      // act
      await sut.RunAsync(CancellationToken.None);

      // assert
      A.CallTo(func).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void RunAsync_TaskThrows_Throws()
    {
      // arrange
      var expectedException = new Exception("I'm failing You, I'm so sorry.");
      Task Func(CancellationToken token) => Task.FromException(expectedException);
      var sut = new SingleProcess(Func);

      // act + assert
      sut.Invoking(async x => await x.RunAsync(CancellationToken.None))
        .Should().Throw<Exception>().Where(ex => ex == expectedException);
    }
  }
}
