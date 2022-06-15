using Moq;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenDeleteConnectionRequest;

public class AndConnectionServiceUnavailable
{
	private readonly ConnectionController _connectionController;

	public AndConnectionServiceUnavailable()
	{
		var connectionServiceMock = new Mock<IConnectionService>();

		connectionServiceMock
			.Setup(service => service.DeletePartnerAsync(
				It.IsAny<string>(),
				true,
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		_connectionController = new ConnectionController(connectionServiceMock.Object, Mock.Of<IBankPartnerConnectionRepository>());
	}

	[Fact]
	public async Task WhenDeleting_ThenThrows() =>
		await FluentActions
			.Awaiting(() => _connectionController.Delete("connection-id"))
			.Should()
			.ThrowAsync<Exception>();
}
