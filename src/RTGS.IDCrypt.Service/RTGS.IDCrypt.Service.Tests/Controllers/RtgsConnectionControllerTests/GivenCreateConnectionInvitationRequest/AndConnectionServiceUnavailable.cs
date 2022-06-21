using Moq;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenCreateConnectionInvitationForRtgsRequest;

public class AndConnectionServiceUnavailable
{
	private readonly RtgsConnectionController _rtgsConnectionController;

	public AndConnectionServiceUnavailable()
	{
		var rtgsConnectionServiceMock = new Mock<IRtgsConnectionService>();

		rtgsConnectionServiceMock
			.Setup(connectionsClient => connectionsClient.CreateInvitationAsync(
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		_rtgsConnectionController = new RtgsConnectionController(rtgsConnectionServiceMock.Object);
	}

	[Fact]
	public async Task WhenPosting_ThenThrows() =>
		await FluentActions
			.Awaiting(() => _rtgsConnectionController.Create())
			.Should()
			.ThrowAsync<Exception>();
}
