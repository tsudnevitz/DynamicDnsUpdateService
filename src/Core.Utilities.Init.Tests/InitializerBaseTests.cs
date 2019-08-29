using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using Core.Utilities.Init.Attributes;
using Core.Utilities.Init.Enums;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Core.Utilities.Init.Tests
{
  public class InitializerBaseTests
  {
    private readonly Fixture _fixture;

    [StartMode(RunModes.Ending)]
    [StopMode(RunModes.Beginning)]
    [OnStartRunAfter(typeof(int))]
    [OnStartRunAfter(typeof(long))]
    [OnStopRunAfter(typeof(bool))]
    [OnStopRunAfter(typeof(string))]
    private class SutInitializer : InitializerBase
    {
      protected override Task OnStartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
      protected override Task OnStopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public InitializerBaseTests()
    {
      _fixture = new Fixture();
      _fixture.Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
    }

    [Fact]
    public void Ctor_NoAttributes_InitialValuesAreCorrect()
    {
      // arrange
      var sut = _fixture.Create<InitializerBase>();

      // assert
      sut.StartMode.Should().Be(RunModes.Beginning);
      sut.StopMode.Should().Be(RunModes.Ending);
      sut.OnStartRunAfter.Should().BeEmpty();
      sut.OnStopRunAfter.Should().BeEmpty();
    }

    [Fact]
    public void Ctor_AttributesSet_ValuesAreCorrect()
    {
      // arrange
      var sut = new SutInitializer();

      // assert
      sut.StartMode.Should().Be(RunModes.Ending);
      sut.StopMode.Should().Be(RunModes.Beginning);
      sut.OnStartRunAfter.Should().BeEquivalentTo(typeof(int), typeof(long));
      sut.OnStopRunAfter.Should().BeEquivalentTo(typeof(bool), typeof(string));
    }

    [Fact]
    public void StartAsync_OverrideDoesNotThrow_ShouldNotThrow()
    {
      // arrange
      var sut = _fixture.Create<InitializerBase>();

      // act + assert
      sut.Invoking(async x => await x.StartAsync(CancellationToken.None))
        .Should().NotThrow();
    }

    [Fact]
    public async Task StopAsync_OverrideDoesNotThrow_ShouldNotThrow()
    {
      // arrange
      var sut = _fixture.Create<InitializerBase>();
      await sut.StartAsync(CancellationToken.None);

      // act + assert
      sut.Invoking(async x => await x.StopAsync(CancellationToken.None))
        .Should().NotThrow();
    }

    [Fact]
    public async Task StartAsync_AlreadyStarted_ShouldThrow()
    {
      // arrange
      var sut = _fixture.Create<InitializerBase>();

      // act
      await sut.StartAsync(CancellationToken.None);

      // assert
      sut.Invoking(async x => await x.StartAsync(CancellationToken.None))
        .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void StopAsync_BeforeStart_ShouldThrow()
    {
      // arrange
      var sut = _fixture.Create<InitializerBase>();

      // act + assert
      sut.Invoking(async x => await x.StopAsync(CancellationToken.None))
        .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task StopAsync_AlreadyStopped_ShouldThrow()
    {
      // arrange
      var sut = _fixture.Create<InitializerBase>();
      await sut.StartAsync(CancellationToken.None);

      // act
      await sut.StopAsync(CancellationToken.None);

      // assert
      sut.Invoking(async x => await x.StopAsync(CancellationToken.None))
        .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void StartAsync_OverrideThrows_ShouldThrow()
    {
      // arrange
      var sut = _fixture.Create<InitializerBase>();
      var expectedException = new Exception("I shall fail.");
      A.CallTo(sut).Where(x => x.Method.Name == "OnStartAsync").WithReturnType<Task>()
        .ThrowsAsync(expectedException);

      // act + assert
      sut.Invoking(async x => await x.StartAsync(CancellationToken.None))
        .Should().Throw<Exception>().Where(ex => ex == expectedException);
    }

    [Fact]
    public async void StopAsync_OverrideThrows_ShouldThrow()
    {
      // arrange
      var sut = _fixture.Create<InitializerBase>();
      var expectedException = new Exception("I shall fail.");
      A.CallTo(sut).Where(x => x.Method.Name == "OnStopAsync").WithReturnType<Task>()
        .ThrowsAsync(expectedException);
      await sut.StartAsync(CancellationToken.None);

      // act + assert
      sut.Invoking(async x => await x.StopAsync(CancellationToken.None))
        .Should().Throw<Exception>().Where(ex => ex == expectedException);
    }
  }
}
