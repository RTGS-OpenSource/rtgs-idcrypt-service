using Microsoft.AspNetCore.Mvc;
using Moq;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenCreateConnectionInvitationRequest;

public class AndConnectionServiceAvailable : IAsyncLifetime
{
	private readonly Mock<IConnectionService> _connectionServiceMock;
	private readonly ConnectionController _connectionController;

	private const string Alias = "alias";
	private const string PublicDid = "public-did";

	private IActionResult _response;

	public AndConnectionServiceAvailable()
	{
		_connectionServiceMock = new Mock<IConnectionService>();

		var connectionInvitation = new ConnectionInvitation
		{
			Alias = Alias,
			PublicDid = PublicDid,
			InvitationUrl = "invitation-url",
			Did = "did",
			Type = "type",
			Label = "label",
			RecipientKeys = new[] { "recipient-key-1" },
			ServiceEndpoint = "service-endpoint",
			ImageUrl = "image-url",
			Id = "id"
		};

		_connectionServiceMock
			.Setup(service => service.CreateConnectionInvitationAsync(
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(connectionInvitation)
			.Verifiable();

		_connectionController = new ConnectionController(_connectionServiceMock.Object);
	}

	public async Task InitializeAsync() =>
		_response = await _connectionController.Post();

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPosting_ThenReturnOkResponseWithExpected()
	{
		var createConnectionInvitationResponse = new Contracts.Connection.CreateConnectionInvitationResponse
		{
			Alias = Alias,
			AgentPublicDid = PublicDid,
			InvitationUrl = "invitation-url",
			Invitation = new Contracts.Connection.ConnectionInvitation
			{
				Id = "id",
				Type = "type",
				Label = "label",
				RecipientKeys = new[] { "recipient-key-1" },
				ServiceEndpoint = "service-endpoint",
				ImageUrl = "image-url",
				Did = "did"
			}
		};

		_response.Should().BeOfType<OkObjectResult>()
			.Which.Value.Should().BeEquivalentTo(createConnectionInvitationResponse);
	}

	[Fact]
	public void WhenPosting_ThenCallCreateInvitationAsyncWithExpected() =>
		_connectionServiceMock.Verify();
}
