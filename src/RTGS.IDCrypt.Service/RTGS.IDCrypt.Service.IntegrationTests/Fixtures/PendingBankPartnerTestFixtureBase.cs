using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using RTGS.IDCrypt.Service.Storage;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures;

public abstract class PendingBankPartnerTestFixtureBase : WebApplicationFactory<Program>
{
	private string _pendingBankPartnerConnectionsTableName;

	protected PendingBankPartnerTestFixtureBase()
	{
		LoadConfig();
		CreateTable();
	}

	public IConfigurationRoot Configuration { get; private set; }

	public TableClient PendingBankPartnerConnectionsTable { get; private set; }

	private void LoadConfig() =>
		Configuration = new ConfigurationBuilder()
			.AddJsonFile("testsettings.json")
			.AddEnvironmentVariables()
			.Build();

	private void CreateTable()
	{
		_pendingBankPartnerConnectionsTableName = $"pendingBankPartnerConnections{Guid.NewGuid():N}";

		var storageTableResolver = new StorageTableResolver(Configuration);

		PendingBankPartnerConnectionsTable = storageTableResolver.GetTable(_pendingBankPartnerConnectionsTableName);
	}

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
					new KeyValuePair<string, string>("PendingBankPartnerConnectionsTableName", _pendingBankPartnerConnectionsTableName)
				})
				.Build();

			config.AddConfiguration(testConfig);
		});

		return base.CreateHost(builder);
	}

	protected abstract void CustomiseHost(IHostBuilder builder);
}
