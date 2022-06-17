using Moq;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenCreateConnectionInvitationForRtgsRequest;

public class AndConnectionServiceUnavailable
{
	private readonly ConnectionController _connectionController;

	public AndConnectionServiceUnavailable()
	{
		var rtgsConnectionServiceMock = new Mock<IRtgsConnectionService>();

		rtgsConnectionServiceMock
			.Setup(connectionsClient => connectionsClient.CreateInvitationAsync(
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		_connectionController = new ConnectionController(
			rtgsConnectionServiceMock.Object,
			Mock.Of<IBankConnectionService>(),
			Mock.Of<IBankPartnerConnectionRepository>());
	}

	[Fact]
	public async Task WhenPosting_ThenThrows() =>
		await FluentActions
			.Awaiting(() => _connectionController.ForRtgs())
			.Should()
			.ThrowAsync<Exception>();
}
