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

	protected override async Task Seed()
	{
		await InsertBankPartnerConnectionAsync(new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id-1",
			RowKey = "bank-alias-1",
			Alias = "bank-alias-1",
			ConnectionId = "bank-connection-id-1",
			PublicDid = "bank-public-did-1",
			CreatedAt = DateTime.UtcNow,
			Status = "Pending",
			Role = "Inviter"
		});

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
}
