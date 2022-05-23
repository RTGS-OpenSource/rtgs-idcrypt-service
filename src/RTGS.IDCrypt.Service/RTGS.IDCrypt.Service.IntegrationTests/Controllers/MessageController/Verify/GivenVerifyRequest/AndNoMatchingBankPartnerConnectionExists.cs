using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using RTGS.IDCrypt.Service.Contracts.Message.Verify;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.MessageController.Verify.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Signature;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.MessageController.Verify.GivenVerifyRequest;

public class AndNoMatchingBankPartnerConnectionExists : IClassFixture<NoMatchingBankPartnerConnectionFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly NoMatchingBankPartnerConnectionFixture _testFixture;
	private HttpResponseMessage _httpResponse;

	public AndNoMatchingBankPartnerConnectionExists(NoMatchingBankPartnerConnectionFixture testFixture)
	{
		_testFixture = testFixture;

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		var message = JsonSerializer.SerializeToElement(new { Message = "I am the walrus" });

		var request = new VerifyRequest
		{
			RtgsGlobalId = "rtgs-global-id-2",
			Message = message,
			PrivateSignature = "private-signature",
			Alias = "alias"
		};

		_httpResponse = await _client.PostAsJsonAsync("api/message/verify", request);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void ThenIdCryptAgentIsNotCalled() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Keys.Should().NotContain(VerifyPrivateSignature.Path);

	[Fact]
	public void ThenNotFoundResponseReceived() =>
		_httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
}
