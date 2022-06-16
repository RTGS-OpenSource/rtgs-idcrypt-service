using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;

namespace RTGS.IDCrypt.Service.Repositories;

public class RtgsConnectionRepository : IRtgsConnectionRepository
{
	private readonly IStorageTableResolver _storageTableResolver;
	private readonly ConnectionsConfig _connectionsConfig;
	private readonly ILogger<RtgsConnectionRepository> _logger;
	private readonly IDateTimeProvider _dateTimeProvider;

	public RtgsConnectionRepository(IStorageTableResolver storageTableResolver,
		IOptions<ConnectionsConfig> connectionsOptions,
		ILogger<RtgsConnectionRepository> logger,
		IDateTimeProvider dateTimeProvider)
	{
		_storageTableResolver = storageTableResolver;
		_connectionsConfig = connectionsOptions.Value;
		_logger = logger;
		_dateTimeProvider = dateTimeProvider;
	}

	public async Task CreateAsync(RtgsConnection rtgsConnection, CancellationToken cancellationToken = default)
	{
		try
		{
			rtgsConnection.CreatedAt = _dateTimeProvider.UtcNow;

			var tableClient = _storageTableResolver.GetTable(_connectionsConfig.RtgsConnectionsTableName);

			await tableClient.AddEntityAsync(rtgsConnection, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when saving RTGS connection");

			throw;
		}
	}

	public async Task ActivateAsync(string connectionId, CancellationToken cancellationToken = default)
	{
		try
		{
			var tableClient = _storageTableResolver.GetTable(_connectionsConfig.RtgsConnectionsTableName);

			var connection = tableClient
				.Query<RtgsConnection>(rtgsConnection =>
						rtgsConnection.ConnectionId == connectionId,
					cancellationToken: cancellationToken)
				.SingleOrDefault();

			if (connection is null)
			{
				_logger.LogWarning("Unable to activate connection as the connection was not found");
				return;
			}

			connection.Status = ConnectionStatuses.Active;
			connection.ActivatedAt = _dateTimeProvider.UtcNow;

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

	public async Task<RtgsConnection> GetEstablishedAsync(CancellationToken cancellationToken = default)
	{
		RtgsConnection connection;

		try
		{
			var tableClient = _storageTableResolver.GetTable(_connectionsConfig.RtgsConnectionsTableName);

			var dateThreshold = _dateTimeProvider.UtcNow.Subtract(_connectionsConfig.MinimumConnectionAge);

			var connections = await tableClient
				.QueryAsync<RtgsConnection>(cancellationToken: cancellationToken)
				.Where(rtgsConnection =>
					rtgsConnection.ActivatedAt <= dateThreshold
					&& rtgsConnection.Status == ConnectionStatuses.Active)
				.ToListAsync(cancellationToken);

			connection = connections.MaxBy(rtgsConnection => rtgsConnection.ActivatedAt);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when getting RTGS connection");

			throw;
		}

		if (connection is null)
		{
			const string errorMessage = "No established RTGS connection found";

			var ex = new Exception(errorMessage);

			_logger.LogError(ex, errorMessage);

			throw ex;
		}

		return connection;
	}
}
