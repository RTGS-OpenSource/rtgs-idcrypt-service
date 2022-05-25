using RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;

public class MissingDeleteConnectionFixture : BankPartnerTestFixtureBase
{
	public MissingDeleteConnectionFixture()
	{
		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.WithNotFoundResponse(DeleteConnection.Path)
			.Build();
	}

	public StatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; }

	protected override async Task Seed()
	{
		var aDate = DateTime.SpecifyKind(new(2022, 4, 1, 0, 0, 0), DateTimeKind.Utc);

		var bankPartnerConnections = new List<BankPartnerConnection>
		{
			new()
			{
				PartitionKey = "rtgs-global-id",
				RowKey = "alias",
				ConnectionId = "connection-id-3",
				Alias = "alias",
				CreatedAt = aDate,
				PublicDid = "public-did",
				Status = "Active"
			},
			new()
			{
				PartitionKey = "rtgs-global-id",
				RowKey = "alias",
				ConnectionId = "connection-id-4",
				Alias = "alias",
				CreatedAt = aDate,
				PublicDid = "public-did-1",
				Status = "Pending"
			},
			new()
			{
				PartitionKey = "rtgs-global-id-1",
				RowKey = "alias",
				ConnectionId = "connection-id-5",
				Alias = "alias",
				CreatedAt = aDate,
				PublicDid = "public-did-2",
				Status = "Active"
			}
		};

		foreach (var connection in bankPartnerConnections)
		{
			await InsertBankPartnerConnectionAsync(connection);
		}
	}
	protected override void CustomiseHost(IHostBuilder builder) =>
		builder.ConfigureServices(services =>
			services
				.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
		);
}
