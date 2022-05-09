using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using RTGS.IDCrypt.Service.Contracts.VerifyMessage;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.VerifyController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures;
using Xunit;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.VerifyController;

public class GivenNoMatchingBankPartnerConnectionExists : IClassFixture<NoMatchingBankPartnerConnectionFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly NoMatchingBankPartnerConnectionFixture _testFixture;
	private HttpResponseMessage _httpResponse;

	public GivenNoMatchingBankPartnerConnectionExists(NoMatchingBankPartnerConnectionFixture testFixture)
	{
		_testFixture = testFixture;

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		var request = new VerifyPrivateSignatureRequest()
		{
			RtgsGlobalId = "rtgs-global-id-2",
			Alias = "alias",
			Message = @"{ ""Message"": ""I am the walrus"" }",
			PrivateSignature = "private-signature"
		};

		_httpResponse = await _client.PostAsJsonAsync("api/verify", request);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void ThenIdCryptAgentIsNotCalled()
	{
		using var _ = new AssertionScope();

		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Keys.Should().NotContain(VerifyPrivateSignature.ConnectionsPath);
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Keys.Should().NotContain(VerifyPrivateSignature.VerifyPrivateSignaturePath);
	}

	[Fact]
	public void ThenNotFoundResponseReceived() =>
		_httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
}
