using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures;

public abstract class BankPartnerTestFixtureBase : WebApplicationFactory<Program>
{
	private string _bankPartnerConnectionsTableName;

	protected BankPartnerTestFixtureBase()
	{
		LoadConfig();
		CreateTable();
		Seed();
	}

	public IConfigurationRoot Configuration { get; private set; }

	public TableClient BankPartnerConnectionsTable { get; private set; }

	private void LoadConfig() =>
		Configuration = new ConfigurationBuilder()
			.AddJsonFile("testsettings.json")
			.AddEnvironmentVariables()
			.Build();

	private void CreateTable()
	{
		_bankPartnerConnectionsTableName = $"bankPartnerConnections{Guid.NewGuid():N}";

		var storageTableResolver = new StorageTableResolver(Configuration);

		BankPartnerConnectionsTable = storageTableResolver.GetTable(_bankPartnerConnectionsTableName);
	}

	protected async Task InsertBankPartnerConnectionAsync(BankPartnerConnection bankPartnerConnection) =>
		await BankPartnerConnectionsTable.AddEntityAsync(bankPartnerConnection);

	protected virtual Task Seed() =>
		Task.CompletedTask;

	protected override IHost CreateHost(IHostBuilder builder)
	{
		CustomiseHost(builder);

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

	protected abstract void CustomiseHost(IHostBuilder builder);
}
