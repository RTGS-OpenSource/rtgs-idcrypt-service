using System.Text.Json;
using Moq;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Webhooks.Handlers;
using RTGS.IDCrypt.Service.Webhooks.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.PresentProofHandlerTests;

public class GivenPresentProof
{
	[Fact]
	public async Task WhenPostingRtgsParticipantCredential_ThenSetConnectionActive()
	{
		var connectionRepositoryMock = new Mock<IConnectionRepository>();

		var handler = new PresentProofMessageHandler(connectionRepositoryMock.Object);

		var proof = new Proof
		{
			ConnectionId = "connection-id"
		};

		var serialisedProof = JsonSerializer.Serialize(proof);

		await handler.HandleAsync(serialisedProof, default);

		connectionRepositoryMock.Verify(repo => repo.ActivateBankPartnerConnectionAsync(proof.ConnectionId), Times.Once);
	}
}
