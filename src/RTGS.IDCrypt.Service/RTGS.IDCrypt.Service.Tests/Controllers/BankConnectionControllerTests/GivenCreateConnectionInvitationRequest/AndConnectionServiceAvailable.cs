using Microsoft.AspNetCore.Mvc;
using Moq;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Models.ConnectionInvitations;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Tests.Controllers.BankConnectionControllerTests.GivenCreateConnectionInvitationRequest;

public class AndConnectionServiceAvailable : IAsyncLifetime
{
	private readonly Mock<IBankConnectionService> _bankConnectionServiceMock = new();
	private readonly BankConnectionController _bankConnectionController;
	private readonly CreateConnectionInvitationForBankRequest _createConnectionInvitationRequest;

	private const string Alias = "alias";
	private const string PublicDid = "public-did";

	private IActionResult _response;

	public AndConnectionServiceAvailable()
	{
		_createConnectionInvitationRequest = new CreateConnectionInvitationForBankRequest { RtgsGlobalId = "rtgs-global-id" };

		var connectionInvitation = new BankConnectionInvitation
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

		_bankConnectionServiceMock
			.Setup(service => service.CreateInvitationAsync(
				_createConnectionInvitationRequest.RtgsGlobalId,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(connectionInvitation)
			.Verifiable();

		_bankConnectionController = new BankConnectionController(
			_bankConnectionServiceMock.Object,
			Mock.Of<IBankPartnerConnectionRepository>());
	}

	public async Task InitializeAsync() =>
		_response = await _bankConnectionController.Create(_createConnectionInvitationRequest);

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
		_bankConnectionServiceMock.Verify();
}
