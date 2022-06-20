using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Contracts.Message.Verify;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.MessageControllerTests.GivenVerifyOwnRequest;

public class AndIdCryptAgentAvailable : IAsyncLifetime
{
	private readonly MessageController _controller;
	private readonly VerifyOwnMessageRequest _verifyOwnMessageRequest;
	private readonly Mock<IJsonSignaturesClient> _jsonSignaturesClientMock;
	private IActionResult _response;
	private readonly Mock<IWalletClient> _walletClient;
	private readonly string _publicDid = "public-did-1";

	public AndIdCryptAgentAvailable()
	{
		var message = JsonSerializer.SerializeToElement(new { Message = "I am the walrus" });
		_verifyOwnMessageRequest = new VerifyOwnMessageRequest
		{
			Message = message,
			PublicSignature = "signature"
		};

		_jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();
		_walletClient = new Mock<IWalletClient>();

		_jsonSignaturesClientMock
			.Setup(client => client.VerifyPublicSignatureAsync(
				_verifyOwnMessageRequest.Message,
				_verifyOwnMessageRequest.PublicSignature,
				_publicDid,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(true)
			.Verifiable();

		_walletClient.Setup(client =>
			client.GetPublicDidAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(_publicDid)
			.Verifiable();

		var logger = new FakeLogger<MessageController>();

		_controller = new MessageController(
			logger,
			Mock.Of<IOptions<ConnectionsConfig>>(),
			Mock.Of<IStorageTableResolver>(),
			_jsonSignaturesClientMock.Object,
			Mock.Of<IBankPartnerConnectionRepository>(),
			Mock.Of<IRtgsConnectionRepository>(),
			_walletClient.Object);
	}

	public async Task InitializeAsync() =>
		_response = await _controller.VerifyOwnMessage(_verifyOwnMessageRequest, default);

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPostingVerifyPublicSignatureRequest_ThenCallVerifyPublicSignatureWithExpected() =>
		_jsonSignaturesClientMock.Verify();

	[Fact]
	public void WhenPostingVerifyPublicSignatureRequest_ThenCallGetPublicDid() =>
		_walletClient.Verify();

	[Fact]
	public void WhenPostingVerifyPrivateSignatureRequest_ThenReturnOkResponseWithVerifiedTrue()
	{
		var verifyPublicSignatureResponse = new VerifyOwnMessageResponse
		{
			Verified = true
		};

		_response.Should().BeOfType<OkObjectResult>()
			.Which.Value.Should().BeEquivalentTo(verifyPublicSignatureResponse);
	}
}
