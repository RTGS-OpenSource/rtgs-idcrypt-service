using Moq;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Tests.Controllers.BankConnectionControllerTests.GivenDeleteConnectionRequest;

public class AndConnectionServiceUnavailable
{
	private readonly BankConnectionController _bankConnectionController;

	public AndConnectionServiceUnavailable()
	{
		var bankConnectionServiceMock = new Mock<IBankConnectionService>();

		bankConnectionServiceMock
			.Setup(service => service.DeleteAsync(
				It.IsAny<string>(),
				true,
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		_bankConnectionController = new BankConnectionController(
			bankConnectionServiceMock.Object,
			Mock.Of<IBankPartnerConnectionRepository>());
	}

	[Fact]
	public async Task WhenDeleting_ThenThrows() =>
		await FluentActions
			.Awaiting(() => _bankConnectionController.Delete("connection-id"))
			.Should()
			.ThrowAsync<Exception>();
}
