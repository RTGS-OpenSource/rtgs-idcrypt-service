using Moq;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenCreateConnectionInvitationForRtgsRequest;

public class AndConnectionServiceUnavailable
{
	private readonly ConnectionController _connectionController;

	public AndConnectionServiceUnavailable()
	{
		var connectionServiceMock = new Mock<IConnectionService>();

		connectionServiceMock
			.Setup(connectionsClient => connectionsClient.CreateConnectionInvitationForRtgsAsync(
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		_connectionController = new ConnectionController(connectionServiceMock.Object);
	}

	[Fact]
	public async Task WhenPosting_ThenThrows() =>
		await FluentActions
			.Awaiting(() => _connectionController.ForRtgs())
			.Should()
			.ThrowAsync<Exception>();
}
