using System;
using System.Collections.Generic;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using RTGS.Service.Storage;

namespace RTGS.Service.IntegrationTests.Fixture;

public class TestFixture 
{
	private TableClient _bankPartnerConnectionsTable;

	public TestFixture()
	{
		CreateTable();
	}

	public IConfigurationRoot Config { get; private set; }

	private void CreateTable()
	{
		var bankPartnerConnectionsTableName = $"bankPartnerConnections{Guid.NewGuid():N}";

		Config = new ConfigurationBuilder()
			.AddJsonFile("testsettings.json")
			.AddInMemoryCollection(new[]
			{
				new KeyValuePair<string, string>("BankPartnerConnectionsTableName", bankPartnerConnectionsTableName)
			})
			.Build();

		var _torageTableResolver = new StorageTableResolver(Config);

		_bankPartnerConnectionsTable = _torageTableResolver.GetTable(bankPartnerConnectionsTableName);
	}
}
