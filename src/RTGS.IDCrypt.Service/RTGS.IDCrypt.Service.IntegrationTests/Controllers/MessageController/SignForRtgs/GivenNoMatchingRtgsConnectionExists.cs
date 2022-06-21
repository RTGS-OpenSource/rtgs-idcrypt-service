using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using RTGS.IDCrypt.Service.Contracts.Message.Sign;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.MessageController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Signature;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.MessageController.SignForRtgs;

public class GivenNoMatchingRtgsConnectionExists : IClassFixture<NoMatchingConnectionFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly NoMatchingConnectionFixture _testFixture;
	private HttpResponseMessage _httpResponse;

	public GivenNoMatchingRtgsConnectionExists(NoMatchingConnectionFixture testFixture)
	{
		_testFixture = testFixture;

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		var message = JsonSerializer.SerializeToElement(new { Message = "I am the walrus" });

		var request = new SignMessageForRtgsRequest
		{
			Message = message
		};

		_httpResponse = await _client.PostAsJsonAsync("api/message/sign/for-rtgs", request);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void ThenIdCryptAgentIsNotCalled() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Keys.Should().NotContain(SignDocument.Path);

	[Fact]
	public async Task ThenNotFoundResponseReceived()
	{
		var _ = new AssertionScope();

		_httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

		var content = await _httpResponse.Content.ReadAsStringAsync();

		content.Should().Be("{\"error\":\"No activated bank partner connection found, please try again in a few minutes.\"}");
	}
}
