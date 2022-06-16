using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures;

public abstract class ConnectionsTestFixtureBase : WebApplicationFactory<Program>
{
	private string _bankPartnerConnectionsTableName;
	private string _rtgsConnectionsTableName;

	protected ConnectionsTestFixtureBase()
	{
		LoadConfig();
		CreateTable();
		// need to await the seed method! Flaky tests if we don't.
		Task.Run(async () => await Seed()).Wait();
	}

	public IConfigurationRoot Configuration { get; private set; }

	public TableClient BankPartnerConnectionsTable { get; private set; }

	public TableClient RtgsConnectionsTable { get; private set; }

	private void LoadConfig() =>
		Configuration = new ConfigurationBuilder()
			.AddJsonFile("testsettings.json")
			.AddEnvironmentVariables()
			.Build();

	private void CreateTable()
	{
		var storageTableResolver = new StorageTableResolver(Configuration);

		_bankPartnerConnectionsTableName = $"bankPartnerConnections{Guid.NewGuid():N}";
		BankPartnerConnectionsTable = storageTableResolver.GetTable(_bankPartnerConnectionsTableName);

		_rtgsConnectionsTableName = $"rtgsConnections{Guid.NewGuid():N}";
		RtgsConnectionsTable = storageTableResolver.GetTable(_rtgsConnectionsTableName);
	}

	protected async Task InsertBankPartnerConnectionAsync(BankPartnerConnection bankPartnerConnection) =>
		await BankPartnerConnectionsTable.AddEntityAsync(bankPartnerConnection);

	protected async Task InsertRtgsConnectionAsync(RtgsConnection rtgsConnection) =>
		await RtgsConnectionsTable.AddEntityAsync(rtgsConnection);

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
					new KeyValuePair<string, string>("BankPartneRConnectionsTableName", _bankPartnerConnectionsTableName),
					new KeyValuePair<string, string>("RtgsConnectionsTableName", _rtgsConnectionsTableName)
				})
				.Build();

			config.AddConfiguration(testConfig);
		});

		return base.CreateHost(builder);
	}

	protected virtual void CustomiseHost(IHostBuilder builder)
	{
	}

	protected override void Dispose(bool disposing)
	{
		BankPartnerConnectionsTable.Delete();
		RtgsConnectionsTable.Delete();
		base.Dispose(disposing);
	}
}
