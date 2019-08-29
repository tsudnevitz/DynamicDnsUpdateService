using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using Core.Utilities.Init.Enums;
using Core.Utilities.Init.Processes;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Core.Utilities.Init.Tests
{
  public class BootstrapperTests
  {
    private readonly Bootstrapper _sut;

    private readonly Fixture _fixture;
    private readonly IProcessConstructor _fakeProcessConstructor;
    private readonly IProcess _fakeProcess;

    public BootstrapperTests()
    {
      _fakeProcess = A.Fake<IProcess>();
      _fakeProcessConstructor = A.Fake<IProcessConstructor>();
      A.CallTo(() => _fakeProcessConstructor.BuildStartupProcess(A<IReadOnlyCollection<IInitializer>>._)).Returns(_fakeProcess);
      A.CallTo(() => _fakeProcessConstructor.BuildShutdownProcess(A<IReadOnlyCollection<IInitializer>>._)).Returns(_fakeProcess);

      _fixture = new Fixture();
      _fixture.Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
      _fixture.Inject(_fakeProcessConstructor);
      _sut = _fixture.Create<Bootstrapper>();
    }

    public static IEnumerable<object[]> DelegatesData => 
      new List<object[]>
      {
        new object[]{(Func<Bootstrapper, Task<bool>>) (x => x.BeginStartupAsync(CancellationToken.None)), false}, 
        new object[]{(Func<Bootstrapper, Task<bool>>) (x => x.BeginShutdownAsync(CancellationToken.None)), true}
      };
    
    [Theory]
    [MemberData(nameof(DelegatesData))]
    public async Task StartBeginShutdownAsync_NoProcessSupplied(Func<Bootstrapper, Task<bool>> testDelegate, bool shouldBeStared)
    {
      // arrange
      if (shouldBeStared)
        await _sut.BeginStartupAsync(CancellationToken.None);

      // act
      var result = await testDelegate(_sut);
      Assert.True(result);
    }
    
    [Theory]
    [MemberData(nameof(DelegatesData))]
    public async Task StartBeginShutdownAsync_ProcessSupplied(Func<Bootstrapper, Task<bool>> testDelegate, bool shouldBeStared)
    {
      // arrange
      if (shouldBeStared)
        await _sut.BeginStartupAsync(CancellationToken.None);

      var processToRun = A.Fake<IProcess>();
      A.CallTo(() => processToRun.RunAsync(CancellationToken.None))
        .Returns(Task.CompletedTask);

      A.CallTo(() => _fakeProcessConstructor.BuildStartupProcess(A<IReadOnlyCollection<IInitializer>>._))
        .Returns(processToRun);
      
      // act
      var result = await testDelegate(_sut);

      // assert
      Assert.True(result);
    }
    
    [Theory]
    [MemberData(nameof(DelegatesData))]
    public async Task StartBeginShutdownAsync_FailingProcessSupplied(Func<Bootstrapper, Task<bool>> testDelegate, bool shouldBeStared)
    {
      // arrange
      if (shouldBeStared)
        await _sut.BeginStartupAsync(CancellationToken.None);

      A.CallTo(() => _fakeProcess.RunAsync(CancellationToken.None))
        .Returns(Task.FromException(new Exception("My very purpose of existence is to fail.")));
      
      // act
      var result = await testDelegate(_sut);

      // assert
      Assert.False(result);
    }

    [Theory]
    [MemberData(nameof(DelegatesData))]
    public async Task StartBeginShutdownAsync_LongTask_AdditionalTimeRequested(Func<Bootstrapper, Task<bool>> testDelegate, bool shouldBeStared)
    {
      // arrange
      if (shouldBeStared)
        await _sut.BeginStartupAsync(CancellationToken.None);

      var requestCount = 0;
      
      var tcs = new TaskCompletionSource<bool>();
      A.CallTo(() => _fakeProcess.RunAsync(CancellationToken.None)).Returns(tcs.Task);

      _sut.AdditionalTimeRequested += (sender, span) =>
      {
        if (++requestCount == 2)
          tcs.SetResult(true);
      };

      // act
      var result = await testDelegate(_sut);

      // assert
      Assert.True(result);
    }

    [Fact]
    public async Task BeginStartupAsync_StopIsRunning_ExceptionThrown()
    {
      // arrange
      var shutdownProcess = A.Fake<IProcess>();
      A.CallTo(() => _fakeProcessConstructor.BuildShutdownProcess(A<IReadOnlyCollection<IInitializer>>._)).Returns(shutdownProcess);
      var canEnd = new TaskCompletionSource<bool>();
      var isStarted = new TaskCompletionSource<bool>();

      A.CallTo(() => shutdownProcess.RunAsync(CancellationToken.None)).ReturnsLazily(() =>
      {
        isStarted.SetResult(true);
        return canEnd.Task;
      });

      await _sut.BeginStartupAsync(CancellationToken.None);

      // act
      var stopTask = Task.Factory.StartNew(async () => await _sut.BeginShutdownAsync(CancellationToken.None), TaskCreationOptions.LongRunning);
      await isStarted.Task;
      var sutMethod = _sut.BeginStartupAsync(CancellationToken.None);
      
      // assert
      await Assert.ThrowsAsync<InvalidOperationException>(async () => await sutMethod);
      //// finishing the stop method so no task are left running after all tests are finished
      canEnd.SetResult(true);
      await stopTask;
    }

    [Fact]
    public async Task BeginShutdownAsync_StartIsRunning_ExceptionThrown()
    {
      // arrange
      var startupProcess = A.Fake<IProcess>();
      A.CallTo(() => _fakeProcessConstructor.BuildStartupProcess(A<IReadOnlyCollection<IInitializer>>._)).Returns(startupProcess);
      
      var beginConfirmation = new TaskCompletionSource<bool>();
      var endRequest = new TaskCompletionSource<bool>();

      A.CallTo(() => startupProcess.RunAsync(CancellationToken.None)).ReturnsLazily(() =>
      {
        beginConfirmation.SetResult(true);
        return endRequest.Task;
      });

      // act
      //// forcing to run in different thread
      var startTask = Task.Factory.StartNew(async () => await _sut.BeginStartupAsync(CancellationToken.None), TaskCreationOptions.LongRunning);
      //// awaiting for stop method start confirmation
      await beginConfirmation.Task;

      var stopTask = _sut.BeginShutdownAsync(CancellationToken.None);

      //// finishing the stop method so no task are left running after all tests are finished
      endRequest.SetResult(true);
      await startTask;
      
      // assert
      await Assert.ThrowsAsync<InvalidOperationException>(async () => await stopTask);
    }
    
    [Fact]
    public async Task BeginStartupAsync_StartIsRunning_ExceptionThrown()
    {
      // arrange
      var startupProcess = A.Fake<IProcess>();
      A.CallTo(() => _fakeProcessConstructor.BuildStartupProcess(A<IReadOnlyCollection<IInitializer>>._)).Returns(startupProcess);
      
      var beginConfirmation = new TaskCompletionSource<bool>();
      var endRequest = new TaskCompletionSource<bool>();

      A.CallTo(() => startupProcess.RunAsync(CancellationToken.None)).ReturnsLazily(() =>
      {
        beginConfirmation.SetResult(true);
        return endRequest.Task;
      });

      // act
      //// forcing to run in different thread
      var startTask = Task.Factory.StartNew(async () => await _sut.BeginStartupAsync(CancellationToken.None), TaskCreationOptions.LongRunning);
      //// awaiting for stop method start confirmation
      await beginConfirmation.Task;

      var secondStartTask = _sut.BeginStartupAsync(CancellationToken.None);

      //// finishing the stop method so no task are left running after all tests are finished
      endRequest.SetResult(true);
      await startTask;
      
      // assert
      await Assert.ThrowsAsync<InvalidOperationException>(async () => await secondStartTask);
    }

    [Fact]
    public async Task BeginShutdownAsync_StopIsRunning_ExceptionThrown()
    {
      // arrange
      var shutdownProcess = A.Fake<IProcess>();
      A.CallTo(() => _fakeProcessConstructor.BuildShutdownProcess(A<IReadOnlyCollection<IInitializer>>._)).Returns(shutdownProcess);
      
      var isStarted = new TaskCompletionSource<bool>();
      var shouldFinish = new TaskCompletionSource<bool>();

      A.CallTo(() => shutdownProcess.RunAsync(CancellationToken.None)).ReturnsLazily(() =>
      {
        isStarted.SetResult(true);
        return shouldFinish.Task;
      });

      await _sut.BeginStartupAsync(CancellationToken.None);

      // act
      var stopTask = Task.Factory.StartNew(async () => await _sut.BeginShutdownAsync(CancellationToken.None), TaskCreationOptions.LongRunning);
      //// awaiting for stop method start confirmation
      await isStarted.Task;
      var secondStopTask = _sut.BeginShutdownAsync(CancellationToken.None);
      
      // assert
      await Assert.ThrowsAsync<InvalidOperationException>(async () => await secondStopTask);
      shouldFinish.SetResult(true);
      await stopTask;
    }

    [Fact]
    public void Ctor_InitialStateIsStopped()
    {
      // assert
      _sut.State.Should().Be(BootstrapperStates.Stopped);
    }

    [Fact]
    public async Task BeginStartupAsync_ShouldTransitionToStarting()
    {
      // arrange
      var process = A.Fake<IProcess>();
      A.CallTo(() => _fakeProcessConstructor.BuildStartupProcess(A<IReadOnlyCollection<IInitializer>>._)).Returns(process);
      
      var beginConfirmation = new TaskCompletionSource<bool>();
      var endRequest = new TaskCompletionSource<bool>();

      A.CallTo(() => process.RunAsync(CancellationToken.None)).ReturnsLazily(() =>
      {
        beginConfirmation.SetResult(true);
        return endRequest.Task;
      });

      // act
      using (var monitoredSut = _sut.Monitor())
      {
        //// forcing to run in different thread
        var task = Task.Factory.StartNew(async () => await _sut.BeginStartupAsync(CancellationToken.None), TaskCreationOptions.LongRunning);
        //// awaiting for stop method start confirmation
        await beginConfirmation.Task;

        // assert
        monitoredSut.Should().Raise(nameof(_sut.StateChanged))
          .WithArgs<BootstrapperStates>(x => x == BootstrapperStates.Starting);
        _sut.State.Should().Be(BootstrapperStates.Starting);

        //// finishing the stop method so no task are left running after all tests are finished
        endRequest.SetResult(true);
        await task;
      }
    }

    [Fact]
    public async Task BeginShutdownAsync_ShouldTransitionToStopping()
    {
      // arrange
      var process = A.Fake<IProcess>();
      A.CallTo(() => _fakeProcessConstructor.BuildShutdownProcess(A<IReadOnlyCollection<IInitializer>>._)).Returns(process);
      
      var beginConfirmation = new TaskCompletionSource<bool>();
      var endRequest = new TaskCompletionSource<bool>();

      A.CallTo(() => process.RunAsync(CancellationToken.None)).ReturnsLazily(() =>
      {
        beginConfirmation.SetResult(true);
        return endRequest.Task;
      });

      // act
      using (var monitoredSut = _sut.Monitor())
      {
        await _sut.BeginStartupAsync(CancellationToken.None);
        //// forcing to run in different thread
        var task = Task.Factory.StartNew(async () => await _sut.BeginShutdownAsync(CancellationToken.None), TaskCreationOptions.LongRunning);
        //// awaiting for stop method start confirmation
        await beginConfirmation.Task;

        // assert
        monitoredSut.Should().Raise(nameof(_sut.StateChanged))
          .WithArgs<BootstrapperStates>(x => x == BootstrapperStates.Stopping);
        _sut.State.Should().Be(BootstrapperStates.Stopping);

        //// finishing the stop method so no task are left running after all tests are finished
        endRequest.SetResult(true);
        await task;
      }
    }
    
    [Fact]
    public async Task BeginStartupAsync_ShouldTransitionToStartedEventually()
    {
      // arrange
      var process = A.Fake<IProcess>();
      A.CallTo(() => _fakeProcessConstructor.BuildStartupProcess(A<IReadOnlyCollection<IInitializer>>._)).Returns(process);
      
      var endRequest = new TaskCompletionSource<bool>();
      A.CallTo(() => process.RunAsync(CancellationToken.None)).Returns(endRequest.Task);

      // act
      using (var monitoredSut = _sut.Monitor())
      {
        //// forcing to run in different thread
        var task = Task.Factory.StartNew(async () => await _sut.BeginStartupAsync(CancellationToken.None), TaskCreationOptions.LongRunning);
        endRequest.SetResult(true);
        await task;

        // assert
        monitoredSut.Should().Raise(nameof(_sut.StateChanged))
          .WithArgs<BootstrapperStates>(x => x == BootstrapperStates.Started);
        _sut.State.Should().Be(BootstrapperStates.Started);
      }
    }
    
    [Fact]
    public async Task BeginShutdownAsync_ShouldTransitionToStoppedEventually()
    {
      // arrange
      var process = A.Fake<IProcess>();
      A.CallTo(() => _fakeProcessConstructor.BuildShutdownProcess(A<IReadOnlyCollection<IInitializer>>._)).Returns(process);
      
      var endRequest = new TaskCompletionSource<bool>();
      A.CallTo(() => process.RunAsync(CancellationToken.None)).Returns(endRequest.Task);

      // act
      using (var monitoredSut = _sut.Monitor())
      {
        //// forcing to run in different thread
        await _sut.BeginStartupAsync(CancellationToken.None);
        var task = Task.Factory.StartNew(async () => await _sut.BeginShutdownAsync(CancellationToken.None), TaskCreationOptions.LongRunning);
        endRequest.SetResult(true);
        await task;

        // assert
        monitoredSut.Should().Raise(nameof(_sut.StateChanged))
          .WithArgs<BootstrapperStates>(x => x == BootstrapperStates.Stopping);
        _sut.State.Should().Be(BootstrapperStates.Stopped);
      }
    }
    
    [Theory]
    [MemberData(nameof(DelegatesData))]
    public async Task StartBeginShutdownAsync_Exception_TransitionsToFaulted(Func<Bootstrapper, Task<bool>> testDelegate, bool shouldBeStared)
    {
      // arrange
      if (shouldBeStared)
        await _sut.BeginStartupAsync(CancellationToken.None);

      var fakeProcess = A.Fake<IProcess>();
      A.CallTo(() => fakeProcess.RunAsync(CancellationToken.None)).Returns(Task.FromException(new Exception("My very purpose of existence is to fail.")));
      A.CallTo(() => _fakeProcessConstructor.BuildStartupProcess(A<IReadOnlyCollection<IInitializer>>._)).Returns(fakeProcess);
      A.CallTo(() => _fakeProcessConstructor.BuildShutdownProcess(A<IReadOnlyCollection<IInitializer>>._)).Returns(fakeProcess);

      // act
      using (var monitoredSut = _sut.Monitor())
      {
        await testDelegate(_sut);

        // assert
        monitoredSut.Should().Raise(nameof(_sut.StateChanged))
          .WithArgs<BootstrapperStates>(x => x == BootstrapperStates.Faulted);
        _sut.State.Should().Be(BootstrapperStates.Faulted);
      }
    }

    [Theory]
    [MemberData(nameof(DelegatesData))]
    public async Task StartBeginShutdownAsync_IsFaulted_ExceptionOnInvoke(Func<Bootstrapper, Task<bool>> testDelegate, bool shouldBeStared)
    {
      // arrange
      if (shouldBeStared)
        await _sut.BeginStartupAsync(CancellationToken.None);
      
      var fakeProcess = A.Fake<IProcess>();
      A.CallTo(() => fakeProcess.RunAsync(CancellationToken.None)).Throws<InvalidOperationException>();
      A.CallTo(() => _fakeProcessConstructor.BuildStartupProcess(A<IReadOnlyCollection<IInitializer>>._)).Returns(fakeProcess);
      A.CallTo(() => _fakeProcessConstructor.BuildShutdownProcess(A<IReadOnlyCollection<IInitializer>>._)).Returns(fakeProcess);
      Func<Task> sutMethod = async () => { await testDelegate(_sut); };

      // act
      await testDelegate(_sut).ContinueWith(x =>
      {
        if (!x.IsFaulted) return;
        x.Exception?.Handle(ex => true);
      });

      // assert
      await sutMethod.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [MemberData(nameof(DelegatesData))]
    public async Task StartBeginShutdownAsync_MethodCalledTwice_Throws(Func<Bootstrapper, Task<bool>> testDelegate, bool shouldBeStared)
    {
      // arrange
      if (shouldBeStared)
        await _sut.BeginStartupAsync(CancellationToken.None);

      Func<Task> sutMethod = async () => { await testDelegate(_sut); };
      await testDelegate(_sut);

      // act + assert
      await sutMethod.Should().ThrowAsync<InvalidOperationException>();
    }
  }
}
