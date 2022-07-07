using Microsoft.AspNetCore.Mvc;
using Moq;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Tests.Controllers.BankConnectionControllerTests.GivenDeleteConnectionRequest;

public class AndConnectionServiceAvailable : IAsyncLifetime
{
	private readonly Mock<IBankConnectionService> _bankConnectionServiceMock = new();
	private readonly BankConnectionController _bankConnectionController;
	private const string ConnectionId = "connection-id";

	private IActionResult _response;

	public AndConnectionServiceAvailable()
	{
		_bankConnectionServiceMock
			.Setup(service => service.DeleteAsync(ConnectionId, true, It.IsAny<CancellationToken>()))
			.Verifiable();

		_bankConnectionController = new BankConnectionController(
			_bankConnectionServiceMock.Object,
			Mock.Of<IBankPartnerConnectionRepository>());
	}

	public async Task InitializeAsync() => _response = await _bankConnectionController.Delete(ConnectionId);

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void WhenDeleting_ThenReturnNoContent() => _response.Should().BeOfType<NoContentResult>();

	[Fact]
	public void WhenDeleting_ThenCallServiceDelete() => _bankConnectionServiceMock.Verify();
}
