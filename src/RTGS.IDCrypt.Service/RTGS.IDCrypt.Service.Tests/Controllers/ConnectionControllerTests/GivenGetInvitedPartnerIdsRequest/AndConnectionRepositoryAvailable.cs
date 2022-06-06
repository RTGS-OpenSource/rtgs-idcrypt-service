﻿using Microsoft.AspNetCore.Mvc;
using Moq;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenGetInvitedPartnerIdsRequest;

public class AndConnectionRepositoryAvailable
{
	private readonly Mock<IBankPartnerConnectionRepository> _bankPartnerConnectionRepositoryMock;
	private readonly ConnectionController _connectionController;

	private IActionResult _response;

	public AndConnectionRepositoryAvailable()
	{
		_bankPartnerConnectionRepositoryMock = new Mock<IBankPartnerConnectionRepository>();

		_bankPartnerConnectionRepositoryMock
			.Setup(mock => mock.GetInvitedPartnerIds(It.IsAny<CancellationToken>()))
			.Returns(new[] { "id1", "id2" });

		_connectionController = new ConnectionController(Mock.Of<IConnectionService>(), _bankPartnerConnectionRepositoryMock.Object);

		_response = _connectionController.InvitedPartnerIds();
	}

	[Fact]
	public void WhenInvoked_ThenReturnOkt() => _response.Should().BeOfType<OkObjectResult>();

	[Fact]
	public void WhenInvoked_ThenCallRepositoryAsync() => _bankPartnerConnectionRepositoryMock.Verify();
}
