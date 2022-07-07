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
			.WithOkResponse(new HttpRequestResponseContext("/connections/connection-id-1", string.Empty))
			.WithOkResponse(new HttpRequestResponseContext("/connections/connection-id-2", string.Empty))
			.WithOkResponse(new HttpRequestResponseContext("/connections/connection-id-3", string.Empty))
			.WithOkResponse(new HttpRequestResponseContext("/connections/connection-id-4", string.Empty))
			.WithOkResponse(new HttpRequestResponseContext("/connections/connection-id-5", string.Empty))
			.WithOkResponse(new HttpRequestResponseContext("/connections/rtgs-connection-id-1", string.Empty))
			.WithOkResponse(new HttpRequestResponseContext("/connections/rtgs-connection-id-2", string.Empty))
			.Build();
	}

	public StatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; }

	protected override async Task Seed()
	{
		var createdAt = DateTime.SpecifyKind(new(2022, 4, 1, 0, 0, 0), DateTimeKind.Utc);

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
			CreatedAt = createdAt
		});

		foreach (var connection in bankPartnerConnections)
		{
			await InsertBankPartnerConnectionAsync(connection);
		}

		uniqueIndex = 0;
		var connections = new List<(string alias, string status)>
		{
			("alias-1", "Active"),
			("alias-2", "Pending")
		};
		var rtgsConnections = connections.Select(conn => new RtgsConnection
		{
			PartitionKey = conn.alias,
			RowKey = $"rtgs-connection-id-{++uniqueIndex}",
			Status = conn.status,
			Alias = conn.alias,
			ConnectionId = $"rtgs-connection-id-{uniqueIndex}",
			CreatedAt = createdAt
		});

		foreach (var rtgsConnection in rtgsConnections)
		{
			await InsertRtgsConnectionAsync(rtgsConnection);
		}
	}

	protected override void CustomiseHost(IHostBuilder builder) =>
		builder.ConfigureServices(services =>
			services
				.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
		);
}
