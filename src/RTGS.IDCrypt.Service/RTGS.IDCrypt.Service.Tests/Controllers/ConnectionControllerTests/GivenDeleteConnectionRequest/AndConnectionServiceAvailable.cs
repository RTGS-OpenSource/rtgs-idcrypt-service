using Microsoft.AspNetCore.Mvc;
using Moq;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenDeleteConnectionRequest;

public class AndConnectionServiceAvailable : IAsyncLifetime
{
	private readonly Mock<IConnectionService> _connectionServiceMock;
	private readonly ConnectionController _connectionController;
	private const string ConnectionId = "connection-id";

	private IActionResult _response;

	public AndConnectionServiceAvailable()
	{
		_connectionServiceMock = new Mock<IConnectionService>();

		_connectionServiceMock
			.Setup(service => service.DeleteAsync(ConnectionId, It.IsAny<CancellationToken>()))
			.Verifiable();

		_connectionController = new ConnectionController(_connectionServiceMock.Object);
	}

	public async Task InitializeAsync() => _response = await _connectionController.Delete(ConnectionId);

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void WhenDeleting_ThenReturnNoContent() => _response.Should().BeOfType<NoContentResult>();

	[Fact]
	public void WhenPDeleting_ThenCallSericeDeleteAsync() => _connectionServiceMock.Verify();
}
