using System.Text.Json;
using Moq;
using RTGS.IDCrypt.Service.Contracts.BasicMessage;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Webhooks.Handlers;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.BasicMessage;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.PresentProofHandlerTests;

public class GivenRequestSent : IAsyncLifetime
{
	private readonly Mock<IBankPartnerConnectionRepository> _bankPartnerConnectionRepositoryMock;
	private readonly Proof _presentedProof;
	private readonly Mock<IBasicMessageClient> _basicMessageClient;
	private readonly PresentProofMessageHandler _handler;
	private readonly string _serialisedProof;

	public GivenRequestSent()
	{
		_presentedProof = new Proof
		{
			ConnectionId = "bank-connection-id",
			State = "request_sent"
		};

		_bankPartnerConnectionRepositoryMock = new Mock<IBankPartnerConnectionRepository>();
		_basicMessageClient = new Mock<IBasicMessageClient>();

		_handler = new PresentProofMessageHandler(_bankPartnerConnectionRepositoryMock.Object);

		_serialisedProof = JsonSerializer.Serialize(_presentedProof);
	}

	public async Task InitializeAsync() => await _handler.HandleAsync(_serialisedProof, default);

	public Task DisposeAsync() => Task.CompletedTask;


	[Fact]
	public void ThenConnectionIsNotSetActive() =>
		_bankPartnerConnectionRepositoryMock.Verify(repo =>
			repo.ActivateAsync(_presentedProof.ConnectionId, It.IsAny<CancellationToken>()), Times.Never);

	[Fact]
	public void ThenDoNotNotifyRtgs() =>
		_basicMessageClient.Verify(client =>
			client.SendAsync(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<ApproveBankPartnerRequest>(),
				It.IsAny<CancellationToken>()),
			Times.Never);
}
