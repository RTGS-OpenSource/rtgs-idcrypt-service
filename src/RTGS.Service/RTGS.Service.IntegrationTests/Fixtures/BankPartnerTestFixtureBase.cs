using System;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using RTGS.Service.IntegrationTests.Helpers;
using RTGS.Service.Models;
using RTGS.Service.Storage;

namespace RTGS.Service.IntegrationTests.Fixtures;

public abstract class BankPartnerTestFixtureBase
{
	private TableClient _bankPartnerConnectionsTable;

	public BankPartnerTestFixtureBase()
	{
		LoadConfig();
		CreateTable();
		Seed();
	}

	public IConfigurationRoot Configuration { get; private set; }
	public string BankPartnerConnectionsTableName { get; private set; }
	public StatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; set; }

	private void LoadConfig()
	{
		Configuration = new ConfigurationBuilder()
			.AddJsonFile("testsettings.json")
			.Build();
	}

	private void CreateTable()
	{
		BankPartnerConnectionsTableName = $"bankPartnerConnections{Guid.NewGuid():N}";

		var storageTableResolver = new StorageTableResolver(Configuration);

		_bankPartnerConnectionsTable = storageTableResolver.GetTable(BankPartnerConnectionsTableName);
	}

	public async Task InsertBankPartnerConnectionAsync(BankPartnerConnection bankPartnerConnection) =>
		await _bankPartnerConnectionsTable.AddEntityAsync(bankPartnerConnection);

	public abstract Task Seed();
}
