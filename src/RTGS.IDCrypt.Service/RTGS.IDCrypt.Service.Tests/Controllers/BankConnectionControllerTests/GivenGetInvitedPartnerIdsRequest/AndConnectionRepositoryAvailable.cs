using Microsoft.AspNetCore.Mvc;
using Moq;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Tests.Controllers.BankConnectionControllerTests.GivenGetInvitedPartnerIdsRequest;

public class AndConnectionRepositoryAvailable : IAsyncLifetime
{
	private readonly Mock<IBankPartnerConnectionRepository> _bankPartnerConnectionRepositoryMock;
	private readonly BankConnectionController _bankConnectionController;

	private IActionResult _response;

	public AndConnectionRepositoryAvailable()
	{
		_bankPartnerConnectionRepositoryMock = new Mock<IBankPartnerConnectionRepository>();

		_bankPartnerConnectionRepositoryMock
			.Setup(mock => mock.GetInvitedPartnerIdsAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new[] { "id1", "id2" });

		_bankConnectionController = new BankConnectionController(
			Mock.Of<IBankConnectionService>(),
			_bankPartnerConnectionRepositoryMock.Object);
	}

	public async Task InitializeAsync() => _response = await _bankConnectionController.InvitedPartnerIds();

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void WhenInvoked_ThenReturnOk() => _response.Should().BeOfType<OkObjectResult>();

	[Fact]
	public void WhenInvoked_ThenCallRepositoryAsync() => _bankPartnerConnectionRepositoryMock.Verify();
}
