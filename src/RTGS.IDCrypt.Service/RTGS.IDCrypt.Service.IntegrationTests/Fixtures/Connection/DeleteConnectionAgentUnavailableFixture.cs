using RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;

public class DeleteConnectionAgentUnavailableFixture : ConnectionsTestFixtureBase
{
	public DeleteConnectionAgentUnavailableFixture()
	{
		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.WithServiceUnavailableResponse(DeleteConnection.Path)
			.Build();
	}

	public async Task TestSeed()
	{
		var aDate = DateTime.SpecifyKind(new(2022, 4, 1, 0, 0, 0), DateTimeKind.Utc);

		var bankPartnerConnections = new List<BankPartnerConnection>
		{
			new()
			{
				PartitionKey = "rtgs-global-id",
				RowKey = "alias-1",
				ConnectionId = "connection-id-1",
				Alias = "alias-1",
				CreatedAt = aDate,
				PublicDid = "public-did-1",
				Status = "Pending",
				Role = "Inviter"
			}
		};

		foreach (var connection in bankPartnerConnections)
		{
			await InsertBankPartnerConnectionAsync(connection);
		}
	}

	public StatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; }

	protected override void CustomiseHost(IHostBuilder builder) =>
		builder.ConfigureServices(services =>
			services
				.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
		);
}
