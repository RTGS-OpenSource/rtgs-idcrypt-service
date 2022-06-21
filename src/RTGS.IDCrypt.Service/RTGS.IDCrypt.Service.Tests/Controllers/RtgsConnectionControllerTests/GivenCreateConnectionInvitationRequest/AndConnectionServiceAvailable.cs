using Microsoft.AspNetCore.Mvc;
using Moq;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Models.ConnectionInvitations;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Tests.Controllers.RtgsConnectionControllerTests.GivenCreateConnectionInvitationRequest;

public class AndConnectionServiceAvailable : IAsyncLifetime
{
	private readonly Mock<IRtgsConnectionService> _rtgsConnectionServiceMock = new();
	private readonly RtgsConnectionController _rtgsConnectionController;
	private const string Alias = "alias";
	private const string PublicDid = "public-did";

	private IActionResult _response;

	public AndConnectionServiceAvailable()
	{
		var connectionInvitation = new RtgsConnectionInvitation
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
			Id = "id",
			FromRtgsGlobalId = "rtgs-global-id"
		};

		_rtgsConnectionServiceMock
			.Setup(service => service.CreateInvitationAsync(
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(connectionInvitation)
			.Verifiable();

		_rtgsConnectionController = new RtgsConnectionController(_rtgsConnectionServiceMock.Object);
	}

	public async Task InitializeAsync() =>
		_response = await _rtgsConnectionController.Create();

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPosting_ThenReturnOkResponseWithExpected()
	{
		var createConnectionInvitationResponse = new CreateConnectionInvitationResponse
		{
			FromRtgsGlobalId = "rtgs-global-id",
			Alias = Alias,
			AgentPublicDid = PublicDid,
			InvitationUrl = "invitation-url",
			Invitation = new ConnectionInvitation
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
		_rtgsConnectionServiceMock.Verify();
}
