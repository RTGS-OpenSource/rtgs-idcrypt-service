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

	public BankPartnerConnection ExistingConnection => new BankPartnerConnection
	{
		PartitionKey = "rtgs-global-id",
		RowKey = "alias",
		ConnectionId = "connection-id-1",
		Alias = "alias",
		CreatedAtx = DateTime.UtcNow.Subtract(TimeSpan.Parse(Configuration["MinimumConnectionAge"]).Add(TimeSpan.FromMinutes(1))),
		ActivatedAt = DateTime.UtcNow.Subtract(TimeSpan.Parse(Configuration["MinimumConnectionAge"]).Add(TimeSpan.FromMinutes(1))),
		PublicDid = "public-did",
		Status = "Active",
		Role = "Inviter"
	};

	protected override async Task Seed() =>
		await InsertBankPartnerConnectionAsync(ExistingConnection);

	protected override void CustomiseHost(IHostBuilder builder) =>
		builder.ConfigureServices(services =>
			services.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
		);
}
