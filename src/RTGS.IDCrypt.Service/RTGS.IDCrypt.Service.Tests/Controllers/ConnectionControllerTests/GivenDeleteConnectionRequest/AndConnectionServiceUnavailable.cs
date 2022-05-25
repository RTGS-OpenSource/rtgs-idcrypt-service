﻿using Moq;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenDeleteConnectionRequest;

public class AndConnectionServiceUnavailable
{
	private readonly ConnectionController _connectionController;

	public AndConnectionServiceUnavailable()
	{
		var connectionServiceMock = new Mock<IConnectionService>();

		connectionServiceMock
			.Setup(service => service.DeleteAsync(It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
			.Throws<Exception>();

		_connectionController = new ConnectionController(connectionServiceMock.Object);
	}

	[Fact]
	public async Task WhenDeleting_ThenThrows() =>
		await FluentActions
			.Awaiting(() => _connectionController.Delete("connection-id"))
			.Should()
			.ThrowAsync<Exception>();
}
