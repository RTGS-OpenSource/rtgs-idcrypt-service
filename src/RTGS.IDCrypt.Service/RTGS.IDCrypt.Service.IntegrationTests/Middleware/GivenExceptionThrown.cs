using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures;

namespace RTGS.IDCrypt.Service.IntegrationTests.Middleware;

public class GivenExceptionThrown : IClassFixture<ThrowingFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private HttpResponseMessage _httpResponse;

	public GivenExceptionThrown(ThrowingFixture testFixture)
	{
		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync() =>
		_httpResponse = await _client.PostAsJsonAsync("api/message/sign/for-bank", ThrowingFixture.SignMessageForBankRequest);

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public async Task ThenReturnExpected500Response()
	{
		using var _ = new AssertionScope();

		_httpResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

		var content = await _httpResponse.Content.ReadAsStringAsync();

		content.Should().Be("{\"error\":\"testing middleware with controller\"}");
	}
}
