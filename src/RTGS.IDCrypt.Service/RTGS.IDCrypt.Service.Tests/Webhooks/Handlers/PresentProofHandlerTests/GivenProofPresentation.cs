using System.Text.Json;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Webhooks.Handlers;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.BasicMessage;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.PresentProofHandlerTests;

public class GivenProofPresentation 
{
	private readonly Mock<IBankPartnerConnectionRepository> _bankPartnerConnectionRepositoryMock;
	private readonly Mock<IRtgsConnectionRepository> _rtgsConnectionRepositoryMock;
	private readonly Proof _presentedProof;
	private readonly CoreConfig _coreConfig;
	private readonly Mock<IBasicMessageClient> _basicMessageClient;
	private readonly PresentProofMessageHandler _handler;
	private readonly string _serialisedProof;

	public GivenProofPresentation()
	{
		_presentedProof = new Proof
		{
			ConnectionId = "bank-connection-id"
		};

		_bankPartnerConnectionRepositoryMock = new Mock<IBankPartnerConnectionRepository>();
		
		_rtgsConnectionRepositoryMock = new Mock<IRtgsConnectionRepository>();
		
		_basicMessageClient = new Mock<IBasicMessageClient>();

		_coreConfig = new CoreConfig
		{
			RtgsGlobalId = "accepting-bank-rtgs-global-id"
		};

		_handler = new PresentProofMessageHandler(
			_bankPartnerConnectionRepositoryMock.Object,
			_rtgsConnectionRepositoryMock.Object,
			_basicMessageClient.Object,
			Options.Create(_coreConfig)); 

		_serialisedProof = JsonSerializer.Serialize(_presentedProof);
	}

	[Theory]
	[InlineData("Inviter")]
	[InlineData("Invitee")]
	public async Task ThenSetConnectionActive(string role)
	{
		var bankConnection = new BankPartnerConnection
		{
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

	[Fact]
	public async Task AndBankIsInvitee_ThenNotifyRtgs()
	{
		var bankPartnerConnection = new BankPartnerConnection
		{
			ConnectionId = _presentedProof.ConnectionId,
			Role = "Invitee"
		};

		_bankPartnerConnectionRepositoryMock
			.Setup(repo => repo.GetAsync(_presentedProof.ConnectionId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(bankPartnerConnection);

		var rtgsConnection = new RtgsConnection
		{
			ConnectionId = "rtgs-connection-id"
		};

		_rtgsConnectionRepositoryMock
			.Setup(repo => repo.GetEstablishedAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(rtgsConnection);

		await _handler.HandleAsync(_serialisedProof, default);

		var setBankPartnershipOnlineRequest = new SetBankPartnershipOnlineRequest
		{
			ApprovingBankDid = _coreConfig.RtgsGlobalId,
			RequestingBankDid = "requesting-bank-rtgs-global-id" //TODO - get from proof
		};

		var expectedMessage = new SendBasicMessageRequest
		{
			ConnectionId = rtgsConnection.ConnectionId,
			MessageType = "SetBankPartnershipOnline",
			Content = JsonSerializer.Serialize(setBankPartnershipOnlineRequest)
		};

		Func<SendBasicMessageRequest, bool> messageMatches = actualMessage =>
		{
			actualMessage.Should().BeEquivalentTo(expectedMessage);

			return true;
		};

		_basicMessageClient.Verify(client => 
			client.SendAsync(It.Is<SendBasicMessageRequest>(message => messageMatches(message)), It.IsAny<CancellationToken>()), 
			Times.Once);
	}

	[Fact]
	public async Task AndBankIsInviter_ThenDoNotNotifyRtgs()
	{
		var bankConnection = new BankPartnerConnection
		{
			ConnectionId = _presentedProof.ConnectionId,
			Role = "Inviter"
		};

		_bankPartnerConnectionRepositoryMock
			.Setup(repo => repo.GetAsync(_presentedProof.ConnectionId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(bankConnection);

		await _handler.HandleAsync(_serialisedProof, default);

		var expectedMessage = new SendBasicMessageRequest();

		Func<SendBasicMessageRequest, bool> messageMatches = actualMessage =>
		{
			actualMessage.Should().BeEquivalentTo(expectedMessage);

			return true;
		};

		_basicMessageClient.Verify(client =>
			client.SendAsync(It.Is<SendBasicMessageRequest>(message => messageMatches(message)), It.IsAny<CancellationToken>()),
			Times.Never);
	}
}
