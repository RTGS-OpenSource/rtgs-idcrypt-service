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
		var bankPartnerConnectionRepositoryMock = new Mock<IBankPartnerConnectionRepository>();

		var handler = new PresentProofMessageHandler(bankPartnerConnectionRepositoryMock.Object);

		var proof = new Proof
		{
			ConnectionId = "connection-id"
		};

		var serialisedProof = JsonSerializer.Serialize(proof);

		await handler.HandleAsync(serialisedProof, default);

		bankPartnerConnectionRepositoryMock.Verify(repo =>
			repo.ActivateAsync(proof.ConnectionId, It.IsAny<CancellationToken>()), Times.Once);
	}
}
