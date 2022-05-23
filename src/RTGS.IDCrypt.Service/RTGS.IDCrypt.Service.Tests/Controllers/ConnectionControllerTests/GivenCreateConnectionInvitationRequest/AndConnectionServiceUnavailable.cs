using Moq;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenCreateConnectionInvitationRequest;

public class AndConnectionServiceUnavailable
{
	private readonly ConnectionController _connectionController;

	public AndConnectionServiceUnavailable()
	{
		var connectionServiceMock = new Mock<IConnectionService>();

		connectionServiceMock
			.Setup(connectionsClient => connectionsClient.CreateConnectionInvitationAsync(
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		_connectionController = new ConnectionController(connectionServiceMock.Object);
	}

	[Fact]
	public async Task WhenPosting_ThenThrows()
	{
		var request = new CreateConnectionInvitationRequest
		{
			RtgsGlobalId = "rtgs-global-id"
		};

		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _connectionController.Post(request))
			.Should()
			.ThrowAsync<Exception>();
	}
}
