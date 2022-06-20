using System.Collections.Generic;
using System.Linq.Expressions;
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
			var connection = await GetFromTableAsync(
				bankPartnerConnection => bankPartnerConnection.ConnectionId == connectionId,
				cancellationToken);

			if (connection is null)
			{
				_logger.LogWarning("Unable to activate connection as the bank partner connection was not found");
				return;
			}

			connection.Status = ConnectionStatuses.Active;
			connection.ActivatedAt = _dateTimeProvider.UtcNow;

			var tableClient = _storageTableResolver.GetTable(_connectionsConfig.BankPartnerConnectionsTableName);
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

			var connections
				= await tableClient.QueryAsync<BankPartnerConnection>(
					bankPartnerConnection =>
						bankPartnerConnection.Status == ConnectionStatuses.Active &&
						bankPartnerConnection.Role == ConnectionRoles.Invitee,
					cancellationToken: cancellationToken,
					select: new[] { "PartitionKey" })
				.ToListAsync(cancellationToken);

			return connections
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
			var connection = await GetFromTableAsync(
				bankPartnerConnection => bankPartnerConnection.ConnectionId == connectionId,
				cancellationToken);

			if (connection is null)
			{
				_logger.LogWarning("Unable to delete connection from table storage as the bank partner connection was not found");
				return;
			}

			var tableClient = _storageTableResolver.GetTable(_connectionsConfig.BankPartnerConnectionsTableName);

			await tableClient.DeleteEntityAsync(connection.PartitionKey, connection.RowKey, connection.ETag, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when deleting bank partner connection");

			throw;
		}
	}

	public async Task<BankPartnerConnection> GetActiveAsync(string rtgsGlobalId, string alias, CancellationToken cancellationToken = default)
	{
		BankPartnerConnection connection;

		try
		{
			connection =
				await GetFromTableAsync(
					cnn => cnn.PartitionKey == rtgsGlobalId &&
						   cnn.RowKey == alias &&
						   cnn.Status == ConnectionStatuses.Active, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Error occurred when getting active bank partner connection with RtgsGlobalId {RtgsGlobalId} and Alias {Alias}",
				rtgsGlobalId,
				alias);

			throw;
		}

		if (connection is null)
		{
			var ex = new Exception($"Active Bank partner connection with RtgsGlobalId {rtgsGlobalId} and Alias {alias} not found");

			_logger.LogError(
				ex,
				"Active Bank partner connection with RtgsGlobalId {RtgsGlobalId} and Alias {Alias} not found",
				rtgsGlobalId,
				alias);

			throw ex;
		}

		return connection;
	}

	public async Task<BankPartnerConnection> GetEstablishedAsync(string rtgsGlobalId, CancellationToken cancellationToken = default)
	{
		BankPartnerConnection connection;

		try
		{
			connection = await GetEstablishedFromTableAsync(rtgsGlobalId, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when getting bank partner connection");

			throw;
		}

		if (connection is null)
		{
			var ex = new Exception($"No established bank partner connection with RTGS Global ID {rtgsGlobalId}.");

			_logger.LogError(ex, "No established bank partner connection with RTGS Global ID {RtgsGlobalId}", rtgsGlobalId);

			throw ex;
		}

		return connection;
	}

	private async Task<BankPartnerConnection> GetFromTableAsync(Expression<Func<BankPartnerConnection, bool>> filterExpression, CancellationToken cancellationToken)
	{
		var tableClient = _storageTableResolver.GetTable(_connectionsConfig.BankPartnerConnectionsTableName);

		var connection = await tableClient
			.QueryAsync(filterExpression, cancellationToken: cancellationToken)
			.SingleOrDefaultAsync(cancellationToken);

		return connection;
	}

	private async Task<BankPartnerConnection> GetEstablishedFromTableAsync(string rtgsGlobalId,
		CancellationToken cancellationToken)
	{
		var bankPartnerConnectionsTable =
			_storageTableResolver.GetTable(_connectionsConfig.BankPartnerConnectionsTableName);

		var dateThreshold = _dateTimeProvider.UtcNow.Subtract(_connectionsConfig.MinimumConnectionAge);

		var bankPartnerConnections = await bankPartnerConnectionsTable
			.QueryAsync<BankPartnerConnection>(bankPartnerConnection =>
					bankPartnerConnection.PartitionKey == rtgsGlobalId
					&& bankPartnerConnection.ActivatedAt <= dateThreshold
					&& bankPartnerConnection.Status == ConnectionStatuses.Active,
				cancellationToken: cancellationToken)
			.ToListAsync(cancellationToken);

		var bankPartnerConnection = bankPartnerConnections.MaxBy(connection => connection.ActivatedAt);
		return bankPartnerConnection;
	}
}
