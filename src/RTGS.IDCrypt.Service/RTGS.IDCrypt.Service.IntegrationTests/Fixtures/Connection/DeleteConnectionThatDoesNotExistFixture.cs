using RTGS.IDCrypt.Service.IntegrationTests.Controllers.BankConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;

public class DeleteConnectionThatDoesNotExistFixture : ConnectionsTestFixtureBase
{
	public DeleteConnectionThatDoesNotExistFixture()
	{
		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.WithNotFoundResponse(DeleteConnection.Path)
			.WithOkResponse(SendBasicMessage.HttpRequestResponseContext)
			.Build();
	}

	public StatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; }

	protected override void CustomiseHost(IHostBuilder builder) =>
		builder.ConfigureServices(services =>
			services
				.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
		);
}
