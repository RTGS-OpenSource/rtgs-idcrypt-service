using System.Net.Http;
using System.Net.Http.Json;
using RTGS.IDCrypt.Service.Webhooks.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.IdCryptConnectionMessageHandler;

public class AndAgentAvailable
{
	private readonly HttpClient _client;

	private HttpResponseMessage _httpResponse;

	public AndAgentAvailable()
	{
		testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		var request = new IdCryptConnection
		{ 
		};

		_httpResponse = await _client.PostAsJsonAsync("v1/idcrypt/topic/connection", request);
	}

}
