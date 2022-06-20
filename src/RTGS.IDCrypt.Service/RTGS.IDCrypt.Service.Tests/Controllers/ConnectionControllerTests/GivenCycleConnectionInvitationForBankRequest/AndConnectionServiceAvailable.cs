using Microsoft.AspNetCore.Mvc;
using Moq;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenCycleConnectionInvitationForBankRequest;

public class AndConnectionServiceAvailable : IAsyncLifetime
{
	private readonly Mock<IBankConnectionService> _bankConnectionServiceMock = new();
	private readonly ConnectionController _connectionController;

	private IActionResult _response;
	private const string RtgsGlobalId = "rtgs-global-id";

	public AndConnectionServiceAvailable()
	{
		_bankConnectionServiceMock
			.Setup(service => service.CycleAsync(
				RtgsGlobalId,
				It.IsAny<CancellationToken>()))
			.Verifiable();

		_connectionController = new ConnectionController(
			Mock.Of<IRtgsConnectionService>(),
			_bankConnectionServiceMock.Object,
			Mock.Of<IBankPartnerConnectionRepository>());
	}

	public async Task InitializeAsync()
	{
		var request = new CycleConnectionRequest
		{
			RtgsGlobalId = RtgsGlobalId
		};

		_response = await _connectionController.Cycle(request);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPosting_ThenReturnOkResponseWithExpected() =>
		_response.Should().BeOfType<OkResult>();

	[Fact]
	public void WhenPosting_ThenCallCycleInvitationForBankAsyncWithExpected() =>
		_bankConnectionServiceMock.Verify();
}
