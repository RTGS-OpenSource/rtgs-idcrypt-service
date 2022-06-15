using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCrypt.Service.Webhooks.Models.BasicMessageModels;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.BasicMessage.GivenDeleteConnection;

public class AndAgentUnavailable : IClassFixture<DeleteConnectionAgentBasicMessageApiUnavailableFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private HttpResponseMessage _httpResponse;
	private BasicMessageContent<DeleteBankPartnerConnectionBasicMessage> _message;

	public AndAgentUnavailable(DeleteConnectionAgentBasicMessageApiUnavailableFixture testFixture)
	{
		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		_message = new BasicMessageContent<DeleteBankPartnerConnectionBasicMessage>
		{
			MessageType = nameof(DeleteBankPartnerConnectionBasicMessage),
			MessageContent = new DeleteBankPartnerConnectionBasicMessage()
		};

		var basicMessage = new IdCryptBasicMessage
		{
			ConnectionId = "connection-id",
			Content = JsonSerializer.Serialize(_message),
		};

		_httpResponse = await _client.PostAsJsonAsync("/v1/idcrypt/topic/basicmessages", basicMessage);
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public async Task ThenReturnInternalServerError()
	{
		using var _ = new AssertionScope();

		_httpResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

		var content = await _httpResponse.Content.ReadAsStringAsync();

		content.Should().Be("{\"error\":\"One or more errors occurred. (Error deleting connection from agent)\"}");
	}
}
