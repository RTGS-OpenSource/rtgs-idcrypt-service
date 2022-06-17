using FluentAssertions;
using FluentAssertions.Execution;
using RTGS.IDCrypt.Service.Scheduler.IntegrationTests.Helpers;

namespace RTGS.IDCrypt.Service.Scheduler.IntegrationTests.HostedServices.BankConnectionCyleServicesTests.GivenSchedulerRan;

public class WhenIdCryptServiceCalled : IClassFixture<TestFixture>
{
	private readonly TestFixture _testFixture;

	public WhenIdCryptServiceCalled(TestFixture testFixture)
	{
		_testFixture = testFixture;
	}

	[Fact]
	public async Task ThenBaseAddressSet()
	{
		await _testFixture.RunProgramAsync();

		using var _ = new AssertionScope();

		_testFixture.IdCryptStatusCodeHttpHandler.Requests["/api/connection/InvitedPartnerIds"]
			.RequestUri!.GetLeftPart(UriPartial.Authority)
			.Should().Be(_testFixture.Configuration["IdCryptServiceBaseAddress"]);

		_testFixture.IdCryptStatusCodeHttpHandler.Requests["/api/connection/cycle"]
			.RequestUri!.GetLeftPart(UriPartial.Authority)
			.Should().Be(_testFixture.Configuration["IdCryptServiceBaseAddress"]);
	}
}
