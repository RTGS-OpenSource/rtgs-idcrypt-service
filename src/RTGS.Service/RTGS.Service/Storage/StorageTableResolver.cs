using Azure.Data.Tables;

namespace RTGS.Service.Storage;

public class StorageTableResolver : IStorageTableResolver
{
	private readonly IConfiguration _configuration;

	public StorageTableResolver(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	public TableClient GetTable(string tableName)
	{
		if (string.IsNullOrWhiteSpace(tableName))
		{
			throw new ArgumentException("Value cannot be null or whitespace.", nameof(tableName));
		}

		var connectionString = _configuration.GetValue<string>("AzureWebJobsStorage");

		if (string.IsNullOrWhiteSpace(connectionString))
		{
			throw new InvalidOperationException("AzureWebJobsStorage connection string not in configuration");
		}

		var tableclient = new TableClient(connectionString, tableName);

		tableclient.CreateIfNotExists();

		return tableclient;
	}
}
