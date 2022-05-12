using Microsoft.AspNetCore.Mvc;
using Moq;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenAcceptConnectionInvitationRequest;

public class AndIdCryptApiAvailable : IAsyncLifetime
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock;
	private readonly ConnectionController _connectionController;

	private IActionResult _response;

	public AndIdCryptApiAvailable()
	{
		_connectionsClientMock = new Mock<IConnectionsClient>();

		var connectionResponse = new ConnectionResponse
		{
			Alias = "alias",
			ConnectionId = "connection-id",
			State = "invitation"
		};

		var expectedRequest = new ReceiveAndAcceptInvitationRequest
		{
			Id = "id",
			Type = "type",
			Alias = "alias",
			Label = "label",
			RecipientKeys = new[] { "recipient-key" },
			ServiceEndpoint = "service-endpoint"
		};

		Func<ReceiveAndAcceptInvitationRequest, bool> matches = request =>
		{
			request.Should().BeEquivalentTo(expectedRequest);

			return true;
		};

		_connectionsClientMock
			.Setup(connectionsClient => connectionsClient.ReceiveAndAcceptInvitationAsync(
				It.Is<ReceiveAndAcceptInvitationRequest>(x => matches(x)),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(connectionResponse)
			.Verifiable();

		var logger = new FakeLogger<ConnectionController>();

		_connectionController = new ConnectionController(
			logger,
			_connectionsClientMock.Object,
			Mock.Of<IWalletClient>(),
			Mock.Of<IAliasProvider>());
	}

	public async Task InitializeAsync()
	{
		var request = new AcceptConnectionInvitationRequest(
			"id",
			"type",
			"alias",
			"label",
			new[] { "recipient-key" },
			"service-endpoint");

		_response = await _connectionController.Accept(request, default);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPosting_ThenReturnAccepted() =>
		_response.Should().BeOfType<AcceptedResult>();

	[Fact]
	public void WhenPosting_ThenCallReceiveAndAcceptInvitationAsyncWithExpected() =>
		_connectionsClientMock.Verify();
}
