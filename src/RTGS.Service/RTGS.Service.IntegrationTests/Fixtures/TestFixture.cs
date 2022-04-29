using System;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using RTGS.Service.Storage;

namespace RTGS.Service.IntegrationTests.Fixtures;

public class TestFixture
{
	public string BankPartnerConnectionsTableName => _bankPartnerConnectionsTableName;

	private string _bankPartnerConnectionsTableName;
	private TableClient _bankPartnerConnectionsTable;

	public TestFixture()
	{
		CreateTable();
	}

	private void CreateTable()
	{
		_bankPartnerConnectionsTableName = $"bankPartnerConnections{Guid.NewGuid():N}";

		var _config = new ConfigurationBuilder()
			.AddJsonFile("testsettings.json")
			.Build();

		var _torageTableResolver = new StorageTableResolver(_config);

		_bankPartnerConnectionsTable = _torageTableResolver.GetTable(BankPartnerConnectionsTableName);
	}

	public void InsertBankPartnerConnection()
	{
		
	}
}
