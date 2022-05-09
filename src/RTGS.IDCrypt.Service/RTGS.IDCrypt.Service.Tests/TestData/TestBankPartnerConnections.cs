using System;
using System.Collections.Generic;
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
			Timestamp = DateTime.Parse("2022-04-01")
		},
		new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id-1",
			RowKey = "alias-2",
			ConnectionId = "connection-id-2",
			Timestamp = DateTime.Parse("2022-04-02")
		},
		new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id-2",
			RowKey = "alias-3",
			ConnectionId = "connection-id-3",
			Timestamp = DateTime.Parse("2022-04-03")
		},
		new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id-2",
			RowKey = "alias-4",
			ConnectionId = "connection-id-3",
			Timestamp = DateTime.Parse("2022-04-04")
		}
	};
}
