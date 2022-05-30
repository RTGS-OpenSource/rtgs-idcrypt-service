using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;

namespace RTGS.IDCrypt.Service.Repositories;

public class ConnectionRepository : IConnectionRepository
{
	private readonly IStorageTableResolver _storageTableResolver;
	private readonly ConnectionsConfig _connectionsConfig;
	private readonly ILogger<ConnectionRepository> _logger;

	public ConnectionRepository(IStorageTableResolver storageTableResolver,
		IOptions<ConnectionsConfig> connectionsOptions,
		ILogger<ConnectionRepository> logger)
	{
		_storageTableResolver = storageTableResolver;
		_connectionsConfig = connectionsOptions.Value;
		_logger = logger;
	}

	public async Task ActivateAsync(string connectionId, CancellationToken cancellationToken = default)
	{
		try
		{
			var tableClient = _storageTableResolver.GetTable(_connectionsConfig.BankPartnerConnectionsTableName);

			var connection = tableClient
				.Query<BankPartnerConnection>(cancellationToken: cancellationToken)
				.SingleOrDefault(bankPartnerConnection => bankPartnerConnection.ConnectionId == connectionId);

			if (connection is null)
			{
				_logger.LogWarning("Unable to activate connection as the connection was not found");
				return;
			}

			connection.Status = ConnectionStatuses.Active; 

			await tableClient.UpdateEntityAsync(
				connection,
				connection.ETag,
				TableUpdateMode.Merge,
				cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when activating connection");

			throw;
		}
	}

	public async Task CreateAsync(BankPartnerConnection connection, CancellationToken cancellationToken = default)
	{
		try
		{
			connection.CreatedAt = DateTime.UtcNow;

			var tableClient = _storageTableResolver.GetTable(_connectionsConfig.BankPartnerConnectionsTableName);

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
			var tableClient = _storageTableResolver.GetTable(_connectionsConfig.BankPartnerConnectionsTableName);

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
