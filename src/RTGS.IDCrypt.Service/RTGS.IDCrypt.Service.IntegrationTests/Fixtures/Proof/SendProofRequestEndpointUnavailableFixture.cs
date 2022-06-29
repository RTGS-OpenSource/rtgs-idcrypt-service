using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;
using RTGS.IDCrypt.Service.IntegrationTests.Webhooks.IdCryptConnectionMessageHandler.TestData;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Proof;

public class SendProofRequestEndpointUnavailableFixture : ConnectionsTestFixtureBase
{
	public SendProofRequestEndpointUnavailableFixture()
	{
		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.WithServiceUnavailableResponse(SendProofRequest.Path)
			.Build();
	}

	public StatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; }

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
	}

	protected override void CustomiseHost(IHostBuilder builder) =>
		builder.ConfigureServices(services =>
			services.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
		);
}
