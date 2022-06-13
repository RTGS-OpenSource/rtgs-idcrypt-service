using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCrypt.Service.Webhooks.Models.BasicMessageModels;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.BasicMessage.GivenDeleteConnection;

public class AndConnectionDoesNotExist : IClassFixture<DeleteConnectionFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly DeleteConnectionFixture _testFixture;
	private HttpResponseMessage _httpResponse;
	private BasicMessageContent<DeleteBankPartnerConnectionBasicMessage> _message;

	public AndConnectionDoesNotExist(DeleteConnectionFixture testFixture)
	{
		_testFixture = testFixture;

		_testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		_message = new BasicMessageContent<DeleteBankPartnerConnectionBasicMessage>
		{
			MessageType = nameof(DeleteBankPartnerConnectionBasicMessage),
			MessageContent = new DeleteBankPartnerConnectionBasicMessage
			{
				Alias = "alias-invalid",
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
	public void WhenPosting_ThenAgentIsNotCalled() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().BeEmpty();

	[Fact]
	public void ThenReturnInternalServerError() =>
		_httpResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
}
