using FluentAssertions;
using RTGS.IDCrypt.Service.Scheduler.IntegrationTests.Fixtures;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace RTGS.IDCrypt.Service.Scheduler.IntegrationTests.BankConnectionCycleServicesTests.GivenSchedulerRan;

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
			.Given(Request.Create().WithPath("/api/bank-connection/InvitedPartnerIds"))
			.RespondWith(
				Response.Create()
					.WithStatusCode(200)
					.WithBody("[\"12345\",\"67890\"]")
			);

		_testFixture.Server
			.Given(Request.Create()
				.WithHeader("Content-Type", "application/json; charset=utf-8")
				.WithPath("/api/bank-connection/cycle")
				.WithBody("{\"rtgsGlobalId\":\"12345\"}")
			)
			.RespondWith(
				Response.Create()
					.WithStatusCode(200)
			);

		_testFixture.Server
			.Given(Request.Create()
				.WithHeader("Content-Type", "application/json; charset=utf-8")
				.WithPath("/api/bank-connection/cycle")
				.WithBody("{\"rtgsGlobalId\":\"67890\"}")
			)
			.RespondWith(
				Response.Create()
					.WithStatusCode(200)
			);

		Environment.SetEnvironmentVariable("IdCryptServiceAddress", TestFixture.Url);

		var exitCode = await TestFixture.RunProgramAsync();
		exitCode.Should().Be(0, "exit code should be 0, if not something went wrong");
	}
}
