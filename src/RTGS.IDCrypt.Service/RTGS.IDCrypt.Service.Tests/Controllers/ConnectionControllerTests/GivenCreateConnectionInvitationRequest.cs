using System.Threading;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCryptSDK.Connections;
using Xunit;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests;

public class GivenCreateConnectionInvitationRequest
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock;

	public GivenCreateConnectionInvitationRequest()
	{
		_connectionsClientMock = new Mock<IConnectionsClient>();
	}

	[Fact]
	public void WhenPosting_ThenReturnOkResponse()
	{
		var connectionController = new ConnectionController();

		var response = connectionController.Post();

		response.Should().BeOfType<OkResult>();
	}

	[Fact]
	public void WhenPosting_ThenCallCreateInvitationAsyncWithExpected() =>
		_connectionsClientMock.Verify(connectionsClient =>
			connectionsClient.CreateInvitationAsync(
				It.IsAny<string>(),
				true,
				false,
				false,
				It.IsAny<CancellationToken>()),
			Times.Once);
}
