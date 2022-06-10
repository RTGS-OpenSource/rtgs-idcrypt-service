using Microsoft.AspNetCore.Mvc;
using Moq;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenCycleConnectionInvitationForBankRequest;

public class AndConnectionServiceAvailable : IAsyncLifetime
{
	private readonly Mock<IConnectionService> _connectionServiceMock;
	private readonly ConnectionController _connectionController;

	private IActionResult _response;
	private const string RtgsGlobalId = "rtgs-global-id";

	public AndConnectionServiceAvailable()
	{
		_connectionServiceMock = new Mock<IConnectionService>();

		_connectionServiceMock
			.Setup(service => service.CycleConnectionForBankAsync(
				RtgsGlobalId,
				It.IsAny<CancellationToken>()))
			.Verifiable();

		_connectionController = new ConnectionController(_connectionServiceMock.Object, Mock.Of<IBankPartnerConnectionRepository>());
	}

	public async Task InitializeAsync()
	{
		var request = new CycleConnectionRequest
		{
			RtgsGlobalId = RtgsGlobalId
		};

		_response = _connectionController.Cycle(request);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPosting_ThenReturnOkResponseWithExpected() =>
		_response.Should().BeOfType<OkResult>();

	[Fact]
	public void WhenPosting_ThenCallCycleInvitationForBankAsyncWithExpected() =>
		_connectionServiceMock.Verify();
}
