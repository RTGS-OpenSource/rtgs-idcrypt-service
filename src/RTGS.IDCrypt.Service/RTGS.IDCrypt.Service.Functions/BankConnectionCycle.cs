using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RTGS.IDCrypt.Service.Contracts.Connection;

namespace RTGS.IDCrypt.Service.Functions;

public class BankConnectionCycle
{
	private readonly ILogger<BankConnectionCycle> _logger;
	private readonly HttpClient _httpClient;

	public BankConnectionCycle(ILogger<BankConnectionCycle> logger, IHttpClientFactory clientFactory)
	{
		_logger = logger;
		_httpClient = clientFactory.CreateClient("IdCryptServiceClient");
	}

	[Function(nameof(BankConnectionCycle))]
	[SuppressMessage("Style", "IDE0060:Remove unused parameter")]
	public async Task RunAsync([TimerTrigger("%ConnectionCycleTriggerTime%")] TimerInfo timerInfo)
	{
		_logger.LogInformation("BankConnectionCycle triggered at: {Time}", DateTime.Now);

		string[] partnerIds;

		try
		{
			partnerIds = await _httpClient.GetFromJsonAsync<string[]>("/api/connection/InvitedPartnerIds");
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error while getting partner ids");
			throw;
		}

		var failed = false;

		foreach (var partnerId in partnerIds)
		{
			var cycleConnectionRequest = new CycleConnectionRequest { RtgsGlobalId = partnerId };

			_logger.LogInformation("Cycling connection for {RtgsGlobalId}", partnerId);

			try
			{
				var response = await _httpClient.PostAsJsonAsync("/api/connection/cycle", cycleConnectionRequest);

				response.EnsureSuccessStatusCode();
			}
			catch (Exception exception)
			{
				failed = true;

				_logger.LogError(exception, "Error cycling connection for {RtgsGlobalId}", partnerId);
			}
		}

		if (failed)
		{
			throw new Exception("One or more cycling attempts failed.");
		}
	}
}
