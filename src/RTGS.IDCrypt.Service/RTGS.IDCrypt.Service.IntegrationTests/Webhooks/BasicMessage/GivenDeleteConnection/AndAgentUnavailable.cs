using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCrypt.Service.Webhooks.Models.BasicMessageModels;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.BasicMessage.GivenDeleteConnection;

public class AndAgentUnavailable : IClassFixture<DeleteConnectionAgentUnavailableFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly DeleteConnectionAgentUnavailableFixture _testFixture;
	private HttpResponseMessage _httpResponse;
	private BasicMessageContent<DeleteBankPartnerConnectionBasicMessage> _message;

	public AndAgentUnavailable(DeleteConnectionAgentUnavailableFixture testFixture)
	{
		_testFixture = testFixture;

		_testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		await _testFixture.TestSeed();
		_message = new BasicMessageContent<DeleteBankPartnerConnectionBasicMessage>
		{
			MessageType = nameof(DeleteBankPartnerConnectionBasicMessage),
			MessageContent = new DeleteBankPartnerConnectionBasicMessage
			{
				Alias = "alias-1",
				FromRtgsGlobalId = "rtgs-global-id"
			}
		};

		var basicMessage = new IdCryptBasicMessage
		{
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
