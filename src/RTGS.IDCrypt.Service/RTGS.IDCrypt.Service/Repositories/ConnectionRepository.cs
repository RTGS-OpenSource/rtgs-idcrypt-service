using Microsoft.Extensions.Options;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;

namespace RTGS.IDCrypt.Service.Repositories;

public class ConnectionRepository : IConnectionRepository
{
	private readonly IStorageTableResolver _storageTableResolver;
	private readonly BankPartnerConnectionsConfig _bankPartnerConnectionsConfig;
	private readonly ILogger<ConnectionRepository> _logger;

	public ConnectionRepository(IStorageTableResolver storageTableResolver,
		IOptions<BankPartnerConnectionsConfig> bankPartnerConnectionsOptions,
		ILogger<ConnectionRepository> logger)
	{
		_storageTableResolver = storageTableResolver;
		_bankPartnerConnectionsConfig = bankPartnerConnectionsOptions.Value;
		_logger = logger;
	}

	public async Task SaveAsync(BankPartnerConnection connection, CancellationToken cancellationToken = default)
	{
		try
		{
			connection.CreatedAt = DateTime.UtcNow;

			var tableClient = _storageTableResolver.GetTable(_bankPartnerConnectionsConfig.BankPartnerConnectionsTableName);

			await tableClient.AddEntityAsync(connection, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when saving bank partner connection");

			throw;
		}
	}

	public async Task DeleteAsync(string connectionId, CancellationToken cancellationToken = default)
	{
		try
		{
			var tableClient = _storageTableResolver.GetTable(_bankPartnerConnectionsConfig.BankPartnerConnectionsTableName);

			var connection = tableClient
				.Query<BankPartnerConnection>(cancellationToken: cancellationToken)
				.SingleOrDefault(bankPartnerConnection => bankPartnerConnection.ConnectionId == connectionId);

			if (connection is null)
			{
				_logger.LogWarning("Unable to delete connection from table storage as the connection was not found");
				return;
			}

			await tableClient.DeleteEntityAsync(connection.PartitionKey, connection.RowKey, connection.ETag, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when deleting connection");

			throw;
		}
	}
}
