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

	protected BankPartnerTestFixtureBase()
	{
		LoadConfig();
		CreateTable();
		Seed();
	}

	public IConfigurationRoot Configuration { get; private set; }
	private string _bankPartnerConnectionsTableName;
	public StatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; protected init; }

	private void LoadConfig() =>
		Configuration = new ConfigurationBuilder()
			.AddJsonFile("testsettings.json")
			.AddEnvironmentVariables()
			.AddInMemoryCollection(new[]
			{
				new KeyValuePair<string, string>("BankPartnerConnectionsTableName", _bankPartnerConnectionsTableName)
			})
			.Build();

	private void CreateTable()
	{
		_bankPartnerConnectionsTableName = $"bankPartnerConnections{Guid.NewGuid():N}";

		var storageTableResolver = new StorageTableResolver(Configuration);

		_bankPartnerConnectionsTable = storageTableResolver.GetTable(_bankPartnerConnectionsTableName);
	}

	protected async Task InsertBankPartnerConnectionAsync(BankPartnerConnection bankPartnerConnection) =>
		await _bankPartnerConnectionsTable.AddEntityAsync(bankPartnerConnection);

	protected abstract Task Seed();

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
					new KeyValuePair<string, string>("BankPartnerConnectionsTableName", _bankPartnerConnectionsTableName)
				})
				.Build();

			config.AddConfiguration(testConfig);
		});

		return base.CreateHost(builder);
	}
}
