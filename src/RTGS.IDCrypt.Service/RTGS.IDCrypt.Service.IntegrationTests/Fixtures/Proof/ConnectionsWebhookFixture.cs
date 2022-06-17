using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;
using RTGS.IDCrypt.Service.IntegrationTests.Webhooks.IdCryptConnectionMessageHandler.TestData;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Proof;

public class ConnectionsWebhookFixture : ConnectionsTestFixtureBase
{
	public ConnectionsWebhookFixture()
	{
		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.WithOkResponse(SendProofRequest.HttpRequestResponseContext)
			.Build();
	}

	public StatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; }

	protected override void CustomiseHost(IHostBuilder builder) =>
		builder.ConfigureServices(services =>
			services.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
		);

	protected override async Task Seed() =>
		await InsertRtgsConnectionAsync(new RtgsConnection
		{
			PartitionKey = "alias",
			RowKey = "connection-id",
			Alias = "alias",
			ConnectionId = "connection-id",
			CreatedAt = new DateTime(2000, 01, 01).ToUniversalTime(),
			Status = "Pending"
		});
}
