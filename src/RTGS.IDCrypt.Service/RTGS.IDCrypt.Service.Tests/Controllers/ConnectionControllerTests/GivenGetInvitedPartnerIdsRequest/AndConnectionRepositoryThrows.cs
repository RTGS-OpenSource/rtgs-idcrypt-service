using Moq;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenGetInvitedPartnerIdsRequest;

public class AndConnectionRepositoryThrows
{
	private readonly Mock<IBankPartnerConnectionRepository> _bankPartnerConnectionRepositoryMock;
	private readonly ConnectionController _connectionController;

	public AndConnectionRepositoryThrows()
	{
		_bankPartnerConnectionRepositoryMock = new Mock<IBankPartnerConnectionRepository>();

		_bankPartnerConnectionRepositoryMock
			.Setup(mock => mock.GetInvitedPartnerIdsAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		_connectionController =
			new ConnectionController(Mock.Of<IConnectionService>(), _bankPartnerConnectionRepositoryMock.Object);
	}

	[Fact]
	public async Task WhenInvoking_ThenThrows() =>
		await FluentActions
			.Awaiting(() => _connectionController.InvitedPartnerIds())
			.Should()
			.ThrowAsync<Exception>();
}
