using System.Net.Http.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RTGS.IDCrypt.Service.Contracts.Connection;

namespace RTGS.IDCrypt.Service.Scheduler;

public class BankConnectionCycleService : IHostedService
{
	private readonly ILogger<BankConnectionCycleService> _logger;
	private readonly IHostApplicationLifetime _hostLifetime;
	private readonly HttpClient _httpClient;

	public BankConnectionCycleService(ILogger<BankConnectionCycleService> logger, IHostApplicationLifetime hostLifetime, IHttpClientFactory clientFactory)
	{
		_logger = logger;
		_hostLifetime = hostLifetime;
		_httpClient = clientFactory.CreateClient("IdCryptServiceClient");
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("BankConnectionCycle triggered at: {Time}", DateTime.Now);

		string[] partnerIds;

		try
		{
			partnerIds = await _httpClient.GetFromJsonAsync<string[]>("/api/connection/InvitedPartnerIds", cancellationToken);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error while getting partner ids");
			throw;
		}

		var failed = false;

		await Parallel.ForEachAsync(partnerIds!, cancellationToken, async (partnerId, innerCancellationToken) =>
		{
			var cycleConnectionRequest = new CycleConnectionRequest { RtgsGlobalId = partnerId };

			_logger.LogInformation("Cycling connection for {RtgsGlobalId}", partnerId);

			try
			{
				var response = await _httpClient.PostAsJsonAsync("/api/connection/cycle", cycleConnectionRequest,
					innerCancellationToken);

				response.EnsureSuccessStatusCode();
			}
			catch (Exception exception)
			{
				failed = true;

				_logger.LogError(exception, "Error cycling connection for {RtgsGlobalId}", partnerId);
			}
		});

		if (failed)
		{
			throw new BankConnectionCycleException("One or more cycling attempts failed.");
		}

		_hostLifetime.StopApplication();
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Stopped ID Crypt Service Scheduler");

		return Task.CompletedTask;
	}
}
