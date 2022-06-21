using Moq;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Tests.Controllers.BankConnectionControllerTests.GivenCreateConnectionInvitationRequest;

public class AndConnectionServiceUnavailable
{
	private readonly BankConnectionController _bankConnectionController;

	public AndConnectionServiceUnavailable()
	{
		var bankConnectionServiceMock = new Mock<IBankConnectionService>();

		bankConnectionServiceMock
			.Setup(connectionsClient => connectionsClient.CreateInvitationAsync(
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		_bankConnectionController = new BankConnectionController(
			bankConnectionServiceMock.Object,
			Mock.Of<IBankPartnerConnectionRepository>());
	}

	[Fact]
	public async Task WhenPosting_ThenThrows()
	{
		var request = new CreateConnectionInvitationForBankRequest
		{
			RtgsGlobalId = "rtgs-global-id"
		};

		await FluentActions
			.Awaiting(() => _bankConnectionController.Create(request))
			.Should()
			.ThrowAsync<Exception>();
	}
}
