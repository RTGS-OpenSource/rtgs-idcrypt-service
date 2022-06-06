using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;

public class InvitedPartnerIdsConnectionFixture : ConnectionsTestFixtureBase
{
	protected override async Task Seed()
	{
		var aDate = DateTime.SpecifyKind(new(2022, 4, 1, 0, 0, 0), DateTimeKind.Utc);

		var uniqueIndex = 1;

		var partners = new List<(string id, string status, string role)>
		{
			// partner with multiple active connections as Invitee
			("rtgs-global-id", "Active", "Invitee"),
			("rtgs-global-id", "Active", "Invitee"),
			("rtgs-global-id", "Active", "Inviter"),
			("rtgs-global-id", "Pending", "Inviter"),
			("rtgs-global-id", "Pending", "Invitee"),

			// partner with single active connection as Invitee
			("rtgs-global-id-1", "Active", "Invitee"),
			("rtgs-global-id-1", "Pending", "Invitee"),
			("rtgs-global-id-1", "Active", "Inviter"),

			// partner with no active connections as Invitee
			("rtgs-global-id-2", "Pending", "Invitee"),
			("rtgs-global-id-2", "Active", "Inviter"),
		};

		var bankPartnerConnections = partners.Select(partner => new BankPartnerConnection
		{
			PartitionKey = partner.id,
			RowKey = $"alias-{uniqueIndex++}",
			ConnectionId = $"connection-id-{uniqueIndex}",
			Alias = $"alias-{uniqueIndex}",
			CreatedAt = aDate,
			PublicDid = "public-did",
			Status = partner.status,
			Role = partner.role
		});

		foreach (var connection in bankPartnerConnections)
		{
			await InsertBankPartnerConnectionAsync(connection);
		}
	}
}
