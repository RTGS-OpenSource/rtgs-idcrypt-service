using System.Collections.Generic;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;

namespace RTGS.IDCrypt.Service.Repositories;

public class BankPartnerConnectionRepository : IBankPartnerConnectionRepository
{
	private readonly IStorageTableResolver _storageTableResolver;
	private readonly ConnectionsConfig _connectionsConfig;
	private readonly ILogger<BankPartnerConnectionRepository> _logger;
	private readonly IDateTimeProvider _dateTimeProvider;

	public BankPartnerConnectionRepository(IStorageTableResolver storageTableResolver,
		IOptions<ConnectionsConfig> connectionsOptions,
		ILogger<BankPartnerConnectionRepository> logger,
		IDateTimeProvider dateTimeProvider)
	{
		_storageTableResolver = storageTableResolver;
		_connectionsConfig = connectionsOptions.Value;
		_logger = logger;
		_dateTimeProvider = dateTimeProvider;
	}

	public async Task ActivateAsync(string connectionId, CancellationToken cancellationToken = default)
	{
		try
		{
			var tableClient = _storageTableResolver.GetTable(_connectionsConfig.BankPartnerConnectionsTableName);

			var connection = await GetFromTableAsync(connectionId, cancellationToken);

			if (connection is null)
			{
				_logger.LogWarning("Unable to activate connection as the bank partner connection was not found");
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
			_logger.LogError(ex, "Error occurred when activating bank partner connection");

			throw;
		}
	}

	public async Task<IEnumerable<string>> GetInvitedPartnerIdsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			var tableClient = _storageTableResolver.GetTable(_connectionsConfig.BankPartnerConnectionsTableName);

			return (await tableClient
				.QueryAsync<BankPartnerConnection>(bankPartnerConnection => bankPartnerConnection.Status == "Active"
																	   && bankPartnerConnection.Role == "Invitee",
					cancellationToken: cancellationToken, select: new[] { "PartitionKey" })
				.ToListAsync(cancellationToken))
				.Select(bankPartnerConnection => bankPartnerConnection.PartitionKey)
				.Distinct();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when querying bank partner connections");

			throw;
		}
	}

	public async Task CreateAsync(BankPartnerConnection connection, CancellationToken cancellationToken = default)
	{
		try
		{
			connection.CreatedAt = _dateTimeProvider.UtcNow;

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

			var connection = await GetFromTableAsync(connectionId, cancellationToken);

			if (connection is null)
			{
				_logger.LogWarning("Unable to delete connection from table storage as the bank partner connection was not found");
				return;
			}

			await tableClient.DeleteEntityAsync(connection.PartitionKey, connection.RowKey, connection.ETag, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when deleting bank partner connection");

			throw;
		}
	}

	public async Task<BankPartnerConnection> GetAsync(string connectionId, CancellationToken cancellationToken = default)
	{
		BankPartnerConnection connection;

		try
		{
			connection = await GetFromTableAsync(connectionId, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when getting bank partner connection");

			throw;
		}

		if (connection is null)
		{
			var ex = new Exception($"Bank partner connection with ID {connectionId} not found");

			_logger.LogError(ex, "Bank partner connection with ID {ConnectionId} not found", connectionId);

			throw ex;
		}

		return connection;
	}

	private async Task<BankPartnerConnection> GetFromTableAsync(string connectionId, CancellationToken cancellationToken)
	{
		var tableClient = _storageTableResolver.GetTable(_connectionsConfig.BankPartnerConnectionsTableName);

		var connection = await tableClient
			.QueryAsync<BankPartnerConnection>(bankPartnerConnection =>
					bankPartnerConnection.ConnectionId == connectionId,
				cancellationToken: cancellationToken)
			.SingleOrDefaultAsync(cancellationToken);

		return connection;
	}
}
