using System;
using System.Collections.Generic;
using System.Linq;
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
			CreatedAt = DateTime.Parse("2022-01-01")
		},
		new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id-1",
			RowKey = "alias-2",
			ConnectionId = "connection-id-2",
			CreatedAt = DateTime.Parse("2022-01-02")
		},
		new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id-2",
			RowKey = "alias-3",
			ConnectionId = "connection-id-3",
			CreatedAt = DateTime.Parse("2022-01-03")
		},
		new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id-2",
			RowKey = "alias-4",
			ConnectionId = "connection-id-4",
			CreatedAt = DateTime.Parse("2022-01-04")
		}
	};

	public static string GetConnectionId(string rtgsGlobalId) =>
		Connections.Where(connection => connection.PartitionKey == rtgsGlobalId)
			.OrderByDescending(connection => connection.CreatedAt).First().ConnectionId;
}
