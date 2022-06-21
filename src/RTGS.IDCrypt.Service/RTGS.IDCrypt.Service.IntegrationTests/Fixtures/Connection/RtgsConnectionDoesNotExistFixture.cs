using RTGS.IDCrypt.Service.IntegrationTests.Controllers.BankConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;

public class RtgsConnectionDoesNotExistFixture : ConnectionsTestFixtureBase
{
	public RtgsConnectionDoesNotExistFixture() =>
		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.WithOkResponse(SendBasicMessage.HttpRequestResponseContext)
			.Build();

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

		await InsertBankPartnerConnectionAsync(new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id-2",
			RowKey = "bank-alias-2",
			Alias = "bank-alias-2",
			ConnectionId = "bank-connection-id-2",
			PublicDid = "public-did-2",
			CreatedAt = DateTime.UtcNow,
			Status = "Pending",
			Role = "Invitee"
		});
	}

	protected override void CustomiseHost(IHostBuilder builder) =>
		builder.ConfigureServices(services =>
			services.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
		);
}
