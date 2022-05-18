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

	public async Task SavePendingBankPartnerConnectionAsync(PendingBankPartnerConnection pendingConnection, CancellationToken cancellationToken = default)
	{
		try
		{
			var tableClient = _storageTableResolver.GetTable(_bankPartnerConnectionsConfig.PendingBankPartnerConnectionsTableName);

			await tableClient.AddEntityAsync(pendingConnection, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when saving pending bank partner connection");

			throw;
		}
	}
}
