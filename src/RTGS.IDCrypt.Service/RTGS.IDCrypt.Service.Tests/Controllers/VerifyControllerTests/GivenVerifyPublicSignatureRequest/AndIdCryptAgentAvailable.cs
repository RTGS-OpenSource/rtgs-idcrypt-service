using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Contracts.VerifyMessage;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.VerifyControllerTests.GivenVerifyPublicSignatureRequest;

public class AndIdCryptAgentAvailable : IAsyncLifetime
{
	private readonly VerifyController _controller;
	private readonly VerifyPublicSignatureRequest _verifyPublicSignatureRequest;
	private readonly Mock<IJsonSignaturesClient> _jsonSignaturesClientMock;
	private IActionResult _response;
	private readonly Mock<IWalletClient> _walletClient;
	private readonly string _publicDid = "public-did-1";


	public AndIdCryptAgentAvailable()
	{
		_verifyPublicSignatureRequest = new VerifyPublicSignatureRequest
		{
			Message = "message",
			PublicSignature = "signature",
		};

		_jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();
		_walletClient = new Mock<IWalletClient>();

		_jsonSignaturesClientMock
			.Setup(client => client.VerifyJsonDocumentPublicSignatureAsync(
				_verifyPublicSignatureRequest.Message,
				_verifyPublicSignatureRequest.PublicSignature,
				_publicDid,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(true)
			.Verifiable();

		_walletClient.Setup(client =>
			client.GetPublicDidAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(_publicDid)
			.Verifiable();

		var logger = new FakeLogger<VerifyController>();

		_controller = new VerifyController(
			logger,
			Mock.Of<IOptions<BankPartnerConnectionsConfig>>(),
			Mock.Of<IStorageTableResolver>(),
			_jsonSignaturesClientMock.Object,
			_walletClient.Object);
	}

	public async Task InitializeAsync() =>
		_response = await _controller.VerifyPublicSignature(_verifyPublicSignatureRequest, default);

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
		var verifyPublicSignatureResponse = new VerifyPublicSignatureResponse
		{
			Verified = true
		};

		_response.Should().BeOfType<OkObjectResult>()
			.Which.Value.Should().BeEquivalentTo(verifyPublicSignatureResponse);
	}
}
