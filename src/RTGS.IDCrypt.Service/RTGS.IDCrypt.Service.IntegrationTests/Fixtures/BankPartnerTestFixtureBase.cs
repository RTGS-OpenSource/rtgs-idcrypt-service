using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures;

public abstract class BankPartnerTestFixtureBase : WebApplicationFactory<Program>
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
			.AddEnvironmentVariables()
			.AddInMemoryCollection(new[]
			{
				new KeyValuePair<string, string>("BankPartnerConnectionsTableName", BankPartnerConnectionsTableName)
			})
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

	protected override IHost CreateHost(IHostBuilder builder)
	{
		builder.ConfigureServices(services =>
			services.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
		);

		builder.ConfigureHostConfiguration(config =>
		{
			var testConfig = new ConfigurationBuilder()
				.AddJsonFile("testsettings.json")
				.AddEnvironmentVariables()
				.AddInMemoryCollection(new[]
				{
					new KeyValuePair<string, string>("BankPartnerConnectionsTableName", BankPartnerConnectionsTableName)
				})
				.Build();

			config.AddConfiguration(testConfig);
		});

		return base.CreateHost(builder);
	}
}
