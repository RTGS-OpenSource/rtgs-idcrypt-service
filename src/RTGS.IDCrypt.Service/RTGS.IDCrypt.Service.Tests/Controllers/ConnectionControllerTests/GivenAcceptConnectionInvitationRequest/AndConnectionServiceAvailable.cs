using Microsoft.AspNetCore.Mvc;
using Moq;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCryptSDK.Connections.Models;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenAcceptConnectionInvitationRequest;

public class AndConnectionServiceAvailable : IAsyncLifetime
{
	private readonly Mock<IConnectionService> _connectionServiceMock;
	private readonly ConnectionController _connectionController;

	private IActionResult _response;

	public AndConnectionServiceAvailable()
	{
		_connectionServiceMock = new Mock<IConnectionService>();

		var expectedRequest = new ReceiveAndAcceptInvitationRequest
		{
			Id = "id",
			Type = "type",
			Alias = "alias",
			Label = "label",
			RecipientKeys = new[] { "recipient-key" },
			ServiceEndpoint = "service-endpoint"
		};

		Func<Models.ConnectionInvitation, bool> requestMatches = request =>
		{
			request.Should().BeEquivalentTo(expectedRequest);

			return true;
		};

		_connectionServiceMock
			.Setup(service => service.AcceptInvitationAsync(
				It.Is<Models.ConnectionInvitation>(request => requestMatches(request)),
				It.IsAny<CancellationToken>()))
			.Verifiable();

		_connectionController = new ConnectionController(_connectionServiceMock.Object);
	}

	public async Task InitializeAsync()
	{
		var request = new AcceptConnectionInvitationRequest
		{
			Id = "id",
			Type = "type",
			Alias = "alias",
			Label = "label",
			RecipientKeys = new[] { "recipient-key" },
			ServiceEndpoint = "service-endpoint",
			AgentPublicDid = "agent-public-did"
		};

		_response = await _connectionController.Accept(request);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPosting_ThenReturnAccepted() =>
		_response.Should().BeOfType<AcceptedResult>();

	[Fact]
	public void WhenPosting_ThenCallReceiveAndAcceptInvitationAsyncWithExpected() =>
		_connectionServiceMock.Verify();
}
