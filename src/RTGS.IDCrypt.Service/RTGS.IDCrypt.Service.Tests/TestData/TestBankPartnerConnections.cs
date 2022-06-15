using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.Tests.TestData;

public static class TestBankPartnerConnections
{
	public static IEnumerable<BankPartnerConnection> Connections => new[]
	{
		new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id-1",
			RowKey = "alias-1",
			ConnectionId = "connection-id-1",
			CreatedAtx = DateTime.Parse("2022-01-01"),
			ActivatedAt = DateTime.Parse("2022-01-01"),
			Status = "Active",
			Role = "Inviter"
		},
		new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id-1",
			RowKey = "alias-2",
			ConnectionId = "connection-id-2",
			CreatedAtx = DateTime.Parse("2022-01-02"),
			ActivatedAt = DateTime.Parse("2022-01-02"),
			Status = "Active",
			Role = "Inviter"
		},
		new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id-1",
			RowKey = "alias-2",
			ConnectionId = "connection-id-3",
			CreatedAtx = DateTime.Parse("2022-01-03"),
			Status = "Pending",
			Role = "Inviter"
		},
		new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id-2",
			RowKey = "alias-3",
			ConnectionId = "connection-id-4",
			CreatedAtx = DateTime.Parse("2022-01-03"),
			ActivatedAt = DateTime.Parse("2022-01-03"),
			Status = "Active",
			Role = "Inviter"
		},
		new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id-2",
			RowKey = "alias-4",
			ConnectionId = "connection-id-5",
			CreatedAtx = DateTime.Parse("2022-01-04"),
			ActivatedAt = DateTime.Parse("2022-01-04"),
			Status = "Active",
			Role = "Inviter"
		}
	};
}
