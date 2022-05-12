using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using RTGS.IDCrypt.Service.Contracts.VerifyMessage;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.VerifyControllerTests.TestData;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.VerifyControllerTests.GivenVerifyPublicSignatureRequest;

public class WhenCallingIdCryptAgent : IClassFixture<VerifyPublicSignatureFixture>, IAsyncLifetime
{

	private readonly HttpClient _client;
	private readonly VerifyPublicSignatureFixture _testFixture;
	private HttpResponseMessage _httpResponse;

	public WhenCallingIdCryptAgent(VerifyPublicSignatureFixture testFixture)
	{
		_testFixture = testFixture;

		_testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		var request = new VerifyPublicSignatureRequest
		{
			Message = @"{ ""Message"": ""I am the walrus"" }",
			PublicSignature = "public-signature"
		};

		_httpResponse = await _client.PostAsJsonAsync("api/verify/public", request);
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void WhenCallingIdCryptAgent_ThenBaseAddressIsExpected()
	{
		using var _ = new AssertionScope();

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[GetPublicDid.Path].Single()
			.RequestUri!.GetLeftPart(UriPartial.Authority)
			.Should().Be(_testFixture.Configuration["AgentApiAddress"]);

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[VerifyPublicSignature.Path].Single()
			.RequestUri!.GetLeftPart(UriPartial.Authority)
			.Should().Be(_testFixture.Configuration["AgentApiAddress"]);
	}

	[Fact]
	public void WhenCallingIdCryptAgent_ThenExpectedPathsAreCalled()
	{
		using var _ = new AssertionScope();

		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKey(GetPublicDid.Path);
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKey(VerifyPublicSignature.Path);
	}

	[Fact]
	public async Task WhenCallingIdCryptAgent_ThenBodyIsExpected()
	{
		var requests = _testFixture.IdCryptStatusCodeHttpHandler.Requests;

		var verifyContent = await requests[VerifyPublicSignature.Path].Single().Content!.ReadAsStringAsync();
		verifyContent.Should().Be(@"{""public_did"":""Test Did"",""document"":{""Message"":""I am the walrus""},""signature"":""public-signature""}");
	}

	[Fact]
	public void WhenCallingIdCryptAgent_ThenApiKeyHeadersAreExpected()
	{
		using var _ = new AssertionScope();

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[GetPublicDid.Path].Single()
			.Headers.GetValues("X-API-Key")
			.Should().ContainSingle()
			.Which.Should().Be(_testFixture.Configuration["AgentApiKey"]);

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[VerifyPublicSignature.Path].Single()
			.Headers.GetValues("X-API-Key")
			.Should().ContainSingle()
			.Which.Should().Be(_testFixture.Configuration["AgentApiKey"]);
	}

	[Fact]
	public async Task ThenReturnOkWithSignMessageResponse()
	{
		using var _ = new AssertionScope();

		_httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var actualResponse = await _httpResponse.Content.ReadFromJsonAsync<VerifyPublicSignatureResponse>();

		actualResponse.Should().BeEquivalentTo(new VerifyPublicSignatureResponse
		{
			Verified = true
		});
	}
}
