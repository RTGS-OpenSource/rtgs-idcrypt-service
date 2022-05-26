using RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;

public class DeleteConnectionAgentUnavailableFixture : BankPartnerTestFixtureBase
{
	public DeleteConnectionAgentUnavailableFixture()
	{
		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.WithServiceUnavailableResponse(DeleteConnection.Path)
			.Build();
	}

	public StatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; }

	protected override void CustomiseHost(IHostBuilder builder) =>
		builder.ConfigureServices(services =>
			services
				.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
		);
}
