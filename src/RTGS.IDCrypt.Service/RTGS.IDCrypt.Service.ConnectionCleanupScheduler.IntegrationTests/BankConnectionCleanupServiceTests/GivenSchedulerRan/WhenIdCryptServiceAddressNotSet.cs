using FluentAssertions;
using RTGS.IDCrypt.Service.ConnectionCleanupScheduler.IntegrationTests.Fixtures;

namespace RTGS.IDCrypt.Service.ConnectionCleanupScheduler.IntegrationTests.BankConnectionCleanupServiceTests.GivenSchedulerRan;

public class WhenIdCryptServiceAddressNotSet
{
	[Fact]
	public async Task ThenBaseAddressIsNotSet()
	{
		Environment.SetEnvironmentVariable("IdCryptServiceAddress", string.Empty);

		var exitCode = await TestFixture.RunProgramAsync();
		exitCode.Should().Be(1, "exit code should be 1, if not there is an exception missing");
	}
}
