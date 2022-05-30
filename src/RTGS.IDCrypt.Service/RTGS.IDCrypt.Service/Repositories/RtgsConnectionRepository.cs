﻿using Microsoft.Extensions.Options;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;

namespace RTGS.IDCrypt.Service.Repositories;

public class RtgsConnectionRepository : IRtgsConnectionRepository
{
	private readonly IStorageTableResolver _storageTableResolver;
	private readonly ConnectionsConfig _connectionsConfig;
	private readonly ILogger<RtgsConnectionRepository> _logger;

	public RtgsConnectionRepository(IStorageTableResolver storageTableResolver,
		IOptions<ConnectionsConfig> connectionsOptions,
		ILogger<RtgsConnectionRepository> logger)
	{
		_storageTableResolver = storageTableResolver;
		_connectionsConfig = connectionsOptions.Value;
		_logger = logger;
	}

	public async Task CreateAsync(RtgsConnection connection, CancellationToken cancellationToken = default)
	{
		try
		{
			connection.CreatedAt = DateTime.UtcNow;

			var tableClient = _storageTableResolver.GetTable(_connectionsConfig.RtgsConnectionsTableName);

			await tableClient.AddEntityAsync(connection, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when saving RTGS connection");

			throw;
		}
	}
}
