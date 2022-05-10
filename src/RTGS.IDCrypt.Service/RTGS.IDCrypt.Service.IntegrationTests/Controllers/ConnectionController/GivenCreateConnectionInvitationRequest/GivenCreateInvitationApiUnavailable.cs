using System.Net;
using System.Net.Http;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.GivenAcceptConnectionInvitationRequest;

public class GivenCreateInvitationApiUnavailable : IClassFixture<AcceptInvitationEndpointUnavailableFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly AcceptInvitationEndpointUnavailableFixture _testFixture;

	private HttpResponseMessage _httpResponse;

	public GivenCreateInvitationApiUnavailable(AcceptInvitationEndpointUnavailableFixture testFixture)
	{
		_testFixture = testFixture;

		_testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync() =>
		_httpResponse = await _client.PostAsync("api/connection", null);

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public async Task ThenReturnInternalServerError()
	{
		using var _ = new AssertionScope();

		_httpResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

		var content = await _httpResponse.Content.ReadAsStringAsync();

		content.Should().Be("{\"error\":\"Error creating invitation\"}");
	}
}
