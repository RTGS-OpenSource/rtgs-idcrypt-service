using Moq;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;

namespace RTGS.IDCrypt.Service.Tests.Services.ConnectionService.GivenAcceptInvitationRequest;

public class AndIdCryptApiAvailable : IAsyncLifetime
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock = new();
	private readonly Service.Services.ConnectionService _connectionService;
	private readonly ReceiveAndAcceptInvitationRequest _request;
	private ConnectionResponse _actualResponse;
	private readonly ConnectionResponse _connectionResponse;

	public AndIdCryptApiAvailable()
	{
		_connectionResponse = new ConnectionResponse
		{
			Alias = "alias",
			ConnectionId = "connection-id",
			State = "invitation"
		};

		_request = new ReceiveAndAcceptInvitationRequest
		{
			Id = "id",
			Type = "type",
			Alias = "alias",
			Label = "label",
			RecipientKeys = new[] { "recipient-key" },
			ServiceEndpoint = "service-endpoint"
		};

		Func<ReceiveAndAcceptInvitationRequest, bool> requestMatches = request =>
		{
			request.Should().BeEquivalentTo(_request);

			return true;
		};

		_connectionsClientMock
			.Setup(client => client.ReceiveAndAcceptInvitationAsync(
				It.Is<ReceiveAndAcceptInvitationRequest>(request => requestMatches(request)),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(_connectionResponse)
			.Verifiable();

		var logger = new FakeLogger<Service.Services.ConnectionService>();

		_connectionService = new Service.Services.ConnectionService(
			_connectionsClientMock.Object,
			logger);
	}

	public async Task InitializeAsync() =>
		_actualResponse = await _connectionService.AcceptInvitationAsync(_request);

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenInvoked_ThenExpectedResponse() => _actualResponse.Should().BeEquivalentTo(_connectionResponse);

	[Fact]
	public void WhenInvoked_ThenCallReceiveAndAcceptInvitationAsyncWithExpected() => _connectionsClientMock.Verify();
}
