using Moq;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Tests.Controllers.BankConnectionControllerTests.GivenGetObsoleteConnectionIdsRequest;

public class AndConnectionRepositoryThrows
{
	private readonly BankConnectionController _bankConnectionController;

	public AndConnectionRepositoryThrows()
	{
		var bankPartnerConnectionRepositoryMock = new Mock<IBankPartnerConnectionRepository>();

		bankPartnerConnectionRepositoryMock
			.Setup(mock => mock.GetExpiredInvitationConnectionIdsAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		bankPartnerConnectionRepositoryMock
			.Setup(mock => mock.GetStaleConnectionIdsAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		_bankConnectionController =
			new BankConnectionController(
				Mock.Of<IBankConnectionService>(),
				bankPartnerConnectionRepositoryMock.Object);
	}

	[Fact]
	public async Task WhenInvoking_ThenThrows() =>
		await FluentActions
			.Awaiting(() => _bankConnectionController.ObsoleteConnectionIds())
			.Should()
			.ThrowAsync<Exception>();
}
