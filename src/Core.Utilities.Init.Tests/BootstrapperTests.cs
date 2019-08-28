using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using Core.Utilities.Init.Processes;
using FakeItEasy;
using Xunit;

namespace Core.Utilities.Init.Tests
{
  public class BootstrapperTests
  {
    private readonly Bootstrapper _sut;
    private readonly IProcessBuilder _fakeProcessBuilder;

    public BootstrapperTests()
    {
      var fixture = new Fixture();
      var fakeItEasy = new AutoFakeItEasyCustomization();

      _fakeProcessBuilder = A.Fake<IProcessBuilder>();
      //fakeItEasy.Relay;

      fixture.Customize(fakeItEasy);

      _sut = fixture.Create<Bootstrapper>();
    }

    [Fact(Skip = "Not finished")]
    public async Task StartAsync_T1()
    {
      await _sut.StartAsync();
    }
  }
}
