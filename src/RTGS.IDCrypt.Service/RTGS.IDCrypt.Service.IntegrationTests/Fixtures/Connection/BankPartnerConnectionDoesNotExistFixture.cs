using RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
public class BankPartnerConnectionDoesNotExistFixture : ConnectionsTestFixtureBase
{
	public BankPartnerConnectionDoesNotExistFixture() =>
		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.WithOkResponse(SendBasicMessage.HttpRequestResponseContext)
			.Build();

	public StatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; }

	protected override async Task Seed()
	{
		var minimumConnectionAge = TimeSpan.Parse(Configuration["MinimumConnectionAge"]).Add(TimeSpan.FromMinutes(1));

		await InsertRtgsConnectionAsync(new RtgsConnection
		{
			PartitionKey = "rtgs-alias-1",
			RowKey = "rtgs-connection-id-1",
			Alias = "rtgs-alias-1",
			ConnectionId = "rtgs-connection-id-1",
			CreatedAt = DateTime.UtcNow.Subtract(minimumConnectionAge),
			ActivatedAt = DateTime.UtcNow.Subtract(minimumConnectionAge),
			Status = "Active"
		});
	}

	protected override void CustomiseHost(IHostBuilder builder) =>
		builder.ConfigureServices(services =>
			services.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
		);
}
