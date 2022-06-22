using FluentAssertions;
using RTGS.IDCrypt.Service.ConnectionCleanupScheduler.IntegrationTests.Fixtures;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace RTGS.IDCrypt.Service.ConnectionCleanupScheduler.IntegrationTests.BankConnectionCleanupServiceTests.GivenSchedulerRan;

public class WhenIdCryptServiceAddressSet : IClassFixture<TestFixture>
{
	private readonly TestFixture _testFixture;

	public WhenIdCryptServiceAddressSet(TestFixture testFixture)
	{
		_testFixture = testFixture;
	}

	[Fact]
	public async Task ThenBaseAddressSet()
	{
		_testFixture.Server
			.Given(Request.Create().WithPath("/api/bank-connection/StaleConnectionIds"))
			.RespondWith(
				Response.Create()
					.WithStatusCode(200)
					.WithBody("[\"connection-id-1\",\"connection-id-2\"]")
			);

		_testFixture.Server
			.Given(Request.Create().WithPath("/api/bank-connection/connection-id-*"))
			.RespondWith(Response.Create().WithStatusCode(200));

		Environment.SetEnvironmentVariable("IdCryptServiceAddress", TestFixture.Url);

		var exitCode = await TestFixture.RunProgramAsync();
		exitCode.Should().Be(0, "exit code should be 0, if not something went wrong");
	}
}
