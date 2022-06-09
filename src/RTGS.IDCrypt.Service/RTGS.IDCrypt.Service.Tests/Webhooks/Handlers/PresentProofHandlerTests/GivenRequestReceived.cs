using System.Text.Json;
using Moq;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Webhooks.Handlers;
using RTGS.IDCrypt.Service.Webhooks.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.PresentProofHandlerTests;

public class GivenRequestReceived
{
	private readonly Mock<IBankPartnerConnectionRepository> _bankPartnerConnectionRepositoryMock;

	private readonly Proof _presentedProof;
	private readonly PresentProofMessageHandler _handler;
	private readonly string _serialisedProof;

	public GivenRequestReceived()
	{
		_presentedProof = new Proof
		{
			ConnectionId = "bank-connection-id",
			State = "request_received"
		};

		_bankPartnerConnectionRepositoryMock = new Mock<IBankPartnerConnectionRepository>();

		var rtgsConnectionRepositoryMock = new Mock<IRtgsConnectionRepository>();

		var rtgsConnection = new RtgsConnection
		{
			ConnectionId = "rtgs-connection-id"
		};

		rtgsConnectionRepositoryMock
			.Setup(repo => repo.GetEstablishedAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(rtgsConnection);

		_handler = new PresentProofMessageHandler(
			_bankPartnerConnectionRepositoryMock.Object);

		_serialisedProof = JsonSerializer.Serialize(_presentedProof);
	}

	[Theory]
	[InlineData("Inviter")]
	[InlineData("Invitee")]
	public async Task ThenSetConnectionActive(string role)
	{
		var bankConnection = new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id",
			ConnectionId = _presentedProof.ConnectionId,
			Role = role
		};

		_bankPartnerConnectionRepositoryMock
			.Setup(repo => repo.GetAsync(_presentedProof.ConnectionId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(bankConnection);

		await _handler.HandleAsync(_serialisedProof, default);

		_bankPartnerConnectionRepositoryMock.Verify(repo =>
			repo.ActivateAsync(_presentedProof.ConnectionId, It.IsAny<CancellationToken>()), Times.Once);
	}
}
