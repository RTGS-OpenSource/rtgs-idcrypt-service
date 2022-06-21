using RTGS.IDCrypt.Service.IntegrationTests.Controllers.BankConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;

public class DeleteConnectionAgentBasicMessageApiUnavailableFixture : ConnectionsTestFixtureBase
{
	public DeleteConnectionAgentBasicMessageApiUnavailableFixture()
	{
		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.WithServiceUnavailableResponse(SendBasicMessage.Path)
			.Build();
	}

	public StatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; }

	protected override void CustomiseHost(IHostBuilder builder) =>
		builder.ConfigureServices(services =>
			services
				.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
		);
}
