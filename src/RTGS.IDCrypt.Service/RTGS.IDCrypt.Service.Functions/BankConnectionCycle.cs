using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace RTGS.IDCrypt.Service.Functions;

public class BankConnectionCycle
{
	private readonly ILogger<BankConnectionCycle> _logger;
	private readonly HttpClient _httpClient;

	public BankConnectionCycle(ILogger<BankConnectionCycle> logger, HttpClient httpClient)
	{
		_logger = logger;
		_httpClient = httpClient;
	}
	
	[Function(nameof(BankConnectionCycle))]
	public async Task RunAsync([TimerTrigger("%ConnectionCycleTriggerTime%")] TimerInfo timerInfo, ILogger log)
	{
		_logger.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow}");

		try
		{
			var partnerIds = await _httpClient.GetFromJsonAsync<string[]>("api/connection/InvitedPartnerIds");
			
			
			// call POST to service
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unable to update accounts from {RequestType}", nameof(BankConnectionCycle));
			throw;
		}
	}
}
