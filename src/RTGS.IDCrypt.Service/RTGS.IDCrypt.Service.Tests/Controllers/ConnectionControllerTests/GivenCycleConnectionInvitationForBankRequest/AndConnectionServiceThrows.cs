using Moq;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenCycleConnectionInvitationForBankRequest;

public class AndConnectionServiceThrows
{
	private readonly ConnectionController _connectionController;

	public AndConnectionServiceThrows()
	{
		var connectionServiceMock = new Mock<IConnectionService>();

		connectionServiceMock
			.Setup(connectionsClient => connectionsClient.CreateConnectionInvitationForBankAsync(
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		_connectionController = new ConnectionController(connectionServiceMock.Object, Mock.Of<IBankPartnerConnectionRepository>());
	}

	[Fact]
	public async Task WhenPosting_ThenThrows()
	{
		var request = new CreateConnectionInvitationForBankRequest
		{
			RtgsGlobalId = "rtgs-global-id"
		};

		await FluentActions
			.Awaiting(() => _connectionController.ForBank(request))
			.Should()
			.ThrowAsync<Exception>();
	}
}
