using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
public class PendingConnectionFixture : ConnectionsTestFixtureBase
{
	protected override async Task Seed()
	{
		await InsertBankPartnerConnectionAsync(new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id",
			RowKey = "alias",
			Alias = "alias",
			ConnectionId = "connection-id",
			PublicDid = "public-did",
			CreatedAt = DateTime.UtcNow,
			Status = "Pending"
		});
	}
}
