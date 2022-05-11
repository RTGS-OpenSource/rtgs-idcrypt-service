using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;

namespace RTGS.IDCrypt.Service.Storage;

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

		var connectionString = _configuration.GetValue<string>("StorageConnection");

		if (string.IsNullOrWhiteSpace(connectionString))
		{
			throw new InvalidOperationException("StorageConnection connection string not in configuration");
		}

		var tableclient = new TableClient(connectionString, tableName);

		tableclient.CreateIfNotExists();

		return tableclient;
	}
}
