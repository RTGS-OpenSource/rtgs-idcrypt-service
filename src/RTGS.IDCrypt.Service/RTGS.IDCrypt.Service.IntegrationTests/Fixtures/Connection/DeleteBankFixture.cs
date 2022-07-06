using RTGS.IDCrypt.Service.IntegrationTests.Controllers.BankConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;

public class DeleteBankFixture : ConnectionsTestFixtureBase
{
	public DeleteBankFixture()
	{
		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.WithOkResponse(DeleteConnection.HttpRequestResponseContext)
			.Build();
	}

	public StatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; }

	public async Task TestSeed()
	{
		var aDate = DateTime.SpecifyKind(new(2022, 4, 1, 0, 0, 0), DateTimeKind.Utc);

		var uniqueIndex = 0;

		var partners = new List<(string id, string status)>
		{
			("rtgs-global-id-1", "Active"),
			("rtgs-global-id-2", "Active"),
			("rtgs-global-id-3", "Active"),
			("rtgs-global-id-1", "Pending"),
			("rtgs-global-id-4", "Pending"),
		};

		var bankPartnerConnections = partners.Select(partner => new BankPartnerConnection
		{
			PartitionKey = partner.id,
			RowKey = $"alias-{++uniqueIndex}",
			ConnectionId = $"connection-id-{uniqueIndex}",
			Alias = $"alias-{uniqueIndex}",
			Status = partner.status,
		});

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
