using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using RTGS.IDCrypt.Service.Contracts.Message.Sign;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.MessageController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Signature;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.MessageController.SignForBank;

public class GivenMultipleMatchingBankPartnerConnectionsExist : IClassFixture<MultipleMatchingBankPartnerConnectionFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly MultipleMatchingBankPartnerConnectionFixture _testFixture;
	private HttpResponseMessage _httpResponse;

	public GivenMultipleMatchingBankPartnerConnectionsExist(MultipleMatchingBankPartnerConnectionFixture testFixture)
	{
		_testFixture = testFixture;

		_testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		var message = JsonSerializer.SerializeToElement(new { Message = "I am the walrus" });

		var request = new SignMessageForBankRequest
		{
			RtgsGlobalId = "rtgs-global-id",
			Message = message
		};

		_httpResponse = await _client.PostAsJsonAsync("api/message/sign/for-bank", request);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenCallingIdCryptAgent_ThenBaseAddressIsExpected() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests[SignDocument.Path].Single()
			.RequestUri!.GetLeftPart(UriPartial.Authority)
			.Should().Be(_testFixture.Configuration["AgentApiAddress"]);

	[Fact]
	public void WhenCallingIdCryptAgent_ThenExpectedPathsAreCalled() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKey("/json-signatures/sign");

	[Fact]
	public async Task WhenCallingIdCryptAgent_ThenBodyIsExpected()
	{
		var content = await _testFixture.IdCryptStatusCodeHttpHandler.Requests[SignDocument.Path].Single().Content!.ReadAsStringAsync();
		content.Should().BeEquivalentTo(@"{""connection_id"":""connection-3"",""document"":{""Message"":""I am the walrus""}}");
	}

	[Fact]
	public void WhenCallingIdCryptAgent_ThenApiKeyHeadersAreExpected() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests[SignDocument.Path].Single()
			.Headers.GetValues("X-API-Key")
			.Should().ContainSingle()
			.Which.Should().Be(_testFixture.Configuration["AgentApiKey"]);

	[Fact]
	public async Task ThenReturnOkWithSignMessageResponse()
	{
		using var _ = new AssertionScope();

		_httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var actualResponse = await _httpResponse.Content.ReadFromJsonAsync<SignMessageResponse>();

		actualResponse.Should().BeEquivalentTo(new SignMessageResponse
		{
			Alias = _testFixture.ValidConnection.Alias,
			PairwiseDidSignature = SignDocument.ExpectedResponse.PairwiseDidSignature,
			PublicDidSignature = SignDocument.ExpectedResponse.PublicDidSignature
		});
	}
}
