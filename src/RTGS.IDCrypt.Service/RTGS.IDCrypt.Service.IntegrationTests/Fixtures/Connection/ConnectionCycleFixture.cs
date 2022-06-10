using RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;

public class ConnectionCycleFixture : ConnectionsTestFixtureBase
{
	public ConnectionCycleFixture()
	{
		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.WithOkResponse(CreateInvitation.HttpRequestResponseContext)
			.WithOkResponse(GetPublicDid.HttpRequestResponseContext)
			.WithOkResponse(ReceiveInvitation.HttpRequestResponseContext)
			.WithOkResponse(AcceptInvitation.HttpRequestResponseContext)
			.WithOkResponse(SendBasicMessage.HttpRequestResponseContext)
			.Build();
	}

	public StatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; }

	protected override async Task Seed()
	{
		var minimumConnectionAge = TimeSpan.Parse(Configuration["MinimumConnectionAge"]).Add(TimeSpan.FromMinutes(1));

		var bankPartnerConnection = new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id",
			RowKey = "alias",
			ConnectionId = "rtgs-connection-id-1",
			Alias = "alias",
			CreatedAt = DateTime.UtcNow.Subtract(minimumConnectionAge),
			PublicDid = "public-did",
			Status = "Active",
			Role = "Inviter"
		};

		await InsertBankPartnerConnectionAsync(bankPartnerConnection);
	}

	protected override void CustomiseHost(IHostBuilder builder) =>
		builder.ConfigureServices(services =>
			services.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
		);
}
