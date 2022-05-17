using Moq;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;

namespace RTGS.IDCrypt.Service.Tests.Services.ConnectionService.GivenCreateInvitationRequest;

public class AndIdCryptApiAvailable : IAsyncLifetime
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock;
	private readonly Service.Services.ConnectionService _connectionService;
	private CreateInvitationResponse _createInvitationResponse;
	private readonly string _alias = "alias";
	private CreateInvitationResponse _actualResponse;

	public AndIdCryptApiAvailable()
	{
		_connectionsClientMock = new Mock<IConnectionsClient>();

		_createInvitationResponse = new CreateInvitationResponse
		{
			ConnectionId = "connection-id",
			Invitation = new ConnectionInvitation
			{
				Id = "id",
				Type = "type",
				Label = "label",
				RecipientKeys = new[]
				{
					"recipient-key-1"
				},
				ServiceEndpoint = "service-endpoint"
			}
		};

		_connectionsClientMock
			.Setup(client => client.CreateInvitationAsync(
				_alias,
				It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(_createInvitationResponse)
			.Verifiable();

		var logger = new FakeLogger<Service.Services.ConnectionService>();

		_connectionService = new Service.Services.ConnectionService(
			_connectionsClientMock.Object,
			logger);
	}

	public async Task InitializeAsync() =>
		_actualResponse = await _connectionService.CreateInvitationAsync(_alias);

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenInvoked_ThenExpectedResponse() => _actualResponse.Should().BeEquivalentTo(_createInvitationResponse);

	[Fact]
	public void WhenInvoked_ThenCallReceiveAndAcceptInvitationAsyncWithExpected() => _connectionsClientMock.Verify();
}
