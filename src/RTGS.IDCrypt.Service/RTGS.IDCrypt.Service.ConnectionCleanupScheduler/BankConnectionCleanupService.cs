using System.Net.Http.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RTGS.IDCrypt.Service.ConnectionCleanupScheduler;

public class BankConnectionCleanupService : IHostedService
{
	private readonly ILogger<BankConnectionCleanupService> _logger;
	private readonly IHostApplicationLifetime _hostLifetime;
	private readonly HttpClient _httpClient;

	public BankConnectionCleanupService(ILogger<BankConnectionCleanupService> logger, IHostApplicationLifetime hostLifetime, IHttpClientFactory clientFactory)
	{
		_logger = logger;
		_hostLifetime = hostLifetime;
		_httpClient = clientFactory.CreateClient("IdCryptServiceClient");
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("BankConnectionCleanup triggered at: {Time}", DateTime.Now);

		string[] connectionIds;

		try
		{
			connectionIds = await _httpClient.GetFromJsonAsync<string[]>("/api/bank-connection/ObsoleteConnectionIds", cancellationToken);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error while getting obsolete connection ids");
			throw;
		}

		var failed = false;

		var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 10, CancellationToken = cancellationToken };

		await Parallel.ForEachAsync(connectionIds!, parallelOptions, async (connectionId, innerCancellationToken) =>
		{
			_logger.LogInformation("Deleting connection {ConnectionId}", connectionId);

			try
			{
				var response = await _httpClient.DeleteAsync($"/api/bank-connection/{connectionId}", innerCancellationToken);

				response.EnsureSuccessStatusCode();
			}
			catch (Exception exception)
			{
				failed = true;

				_logger.LogError(exception, "Error deleting connection for {ConnectionId}", connectionId);
			}
		});

		if (failed)
		{
			throw new BankConnectionCleanupException("One or more delete attempts failed.");
		}

		_hostLifetime.StopApplication();
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Stopped ID Crypt Service Bank Connection Cleanup Scheduler");

		return Task.CompletedTask;
	}
}
