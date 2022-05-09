using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using RTGS.IDCrypt.Service.Contracts.VerifyMessage;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.VerifyControllerTests.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures;
using VerifyXunit;
using Xunit;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.VerifyControllerTests.GivenVerifyPrivateSignatureRequest;

[UsesVerify]
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
		var request = new VerifyPrivateSignatureRequest
		{
			RtgsGlobalId = "rtgs-global-id",
			Alias = "alias",
			Message = @"{ ""Message"": ""I am the walrus"" }",
			PrivateSignature = "private-signature"
		};

		_httpResponse = await _client.PostAsJsonAsync("api/verify", request);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenCallingIdCryptAgent_ThenBaseAddressIsExpected()
	{
		using var _ = new AssertionScope();

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[VerifyPrivateSignature.ConnectionsPath].Single()
			.RequestUri!.GetLeftPart(UriPartial.Authority)
			.Should().Be(_testFixture.Configuration["AgentApiAddress"]);

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[VerifyPrivateSignature.VerifyPrivateSignaturePath].Single()
			.RequestUri!.GetLeftPart(UriPartial.Authority)
			.Should().Be(_testFixture.Configuration["AgentApiAddress"]);
	}

	[Fact]
	public void WhenCallingIdCryptAgent_ThenExpectedPathsAreCalled()
	{
		using var _ = new AssertionScope();

		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKey(VerifyPrivateSignature.ConnectionsPath);
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKey(VerifyPrivateSignature.VerifyPrivateSignaturePath);
	}

	[Fact]
	public async Task WhenCallingIdCryptAgent_ThenBodyIsExpected()
	{
		var requests = _testFixture.IdCryptStatusCodeHttpHandler.Requests;

		var content = await requests[VerifyPrivateSignature.VerifyPrivateSignaturePath].Single().Content!.ReadAsStringAsync();

		await Verifier.Verify(content);
	}

	[Fact]
	public void WhenCallingIdCryptAgent_ThenApiKeyHeadersAreExpected()
	{
		using var _ = new AssertionScope();

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[VerifyPrivateSignature.ConnectionsPath].Single()
			.Headers.GetValues("X-API-Key")
			.Should().ContainSingle()
			.Which.Should().Be(_testFixture.Configuration["AgentApiKey"]);

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[VerifyPrivateSignature.VerifyPrivateSignaturePath].Single()
			.Headers.GetValues("X-API-Key")
			.Should().ContainSingle()
			.Which.Should().Be(_testFixture.Configuration["AgentApiKey"]);
	}

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
