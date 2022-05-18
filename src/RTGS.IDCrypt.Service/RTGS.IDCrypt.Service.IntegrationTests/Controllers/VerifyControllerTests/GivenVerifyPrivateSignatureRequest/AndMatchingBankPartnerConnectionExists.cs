using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using RTGS.IDCrypt.Service.Contracts.VerifyMessage;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.VerifyControllerTests.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Signature;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.VerifyControllerTests.GivenVerifyPrivateSignatureRequest;

public class AndMatchingBankPartnerConnectionExists : IClassFixture<SingleMatchingBankPartnerConnectionFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly SingleMatchingBankPartnerConnectionFixture _testFixture;
	private HttpResponseMessage _httpResponse;

	public AndMatchingBankPartnerConnectionExists(SingleMatchingBankPartnerConnectionFixture testFixture)
	{
		_testFixture = testFixture;
		_testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		var message = JsonSerializer.SerializeToElement(new { Message = "I am the walrus" });

		var request = new VerifyPrivateSignatureRequest
		{
			RtgsGlobalId = "rtgs-global-id",
			Message = message,
			PrivateSignature = "private-signature",
			Alias = "alias"
		};

		_httpResponse = await _client.PostAsJsonAsync("api/verify", request);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenCallingIdCryptAgent_ThenBaseAddressIsExpected() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests[VerifyPrivateSignature.Path].Single()
			.RequestUri!.GetLeftPart(UriPartial.Authority)
			.Should().Be(_testFixture.Configuration["AgentApiAddress"]);

	[Fact]
	public void WhenCallingIdCryptAgent_ThenExpectedPathsAreCalled() => _testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKey(VerifyPrivateSignature.Path);

	[Fact]
	public async Task WhenCallingIdCryptAgent_ThenBodyIsExpected()
	{
		var requests = _testFixture.IdCryptStatusCodeHttpHandler.Requests;

		var content = await requests[VerifyPrivateSignature.Path].Single().Content!.ReadAsStringAsync();
		content.Should().BeEquivalentTo(
			@"{""connection_id"":""connection-id"",""document"":" +
			@"{""Message"":""I am the walrus""},""signature"":""private-signature""}");
	}

	[Fact]
	public void WhenCallingIdCryptAgent_ThenApiKeyHeadersAreExpected() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests[VerifyPrivateSignature.Path].Single()
			.Headers.GetValues("X-API-Key")
			.Should().ContainSingle()
			.Which.Should().Be(_testFixture.Configuration["AgentApiKey"]);

	[Fact]
	public async Task ThenReturnOkWithSignMessageResponse()
	{
		using var _ = new AssertionScope();

		_httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var actualResponse = await _httpResponse.Content.ReadFromJsonAsync<VerifyPrivateSignatureResponse>();

		actualResponse.Should().BeEquivalentTo(new VerifyPrivateSignatureResponse
		{
			Verified = true
		});
	}
}
