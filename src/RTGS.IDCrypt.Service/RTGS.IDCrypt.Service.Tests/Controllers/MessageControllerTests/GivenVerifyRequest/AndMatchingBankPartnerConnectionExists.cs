using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RTGS.IDCrypt.Service.Contracts.Message.Verify;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.MessageControllerTests.GivenVerifyRequest;

public class AndMatchingBankPartnerConnectionExists : IAsyncLifetime
{
	private readonly MessageController _controller;
	private readonly VerifyRequest _verifyRequest;
	private readonly Mock<IJsonSignaturesClient> _jsonSignaturesClientMock;
	private IActionResult _response;

	public AndMatchingBankPartnerConnectionExists()
	{
		var message = JsonSerializer.SerializeToElement(new { Message = "I am the walrus" });

		_verifyRequest = new VerifyRequest
		{
			RtgsGlobalId = "rtgs-global-id-1",
			Message = message,
			PrivateSignature = "signature",
			Alias = "alias-1"
		};

		_jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();

		var bankPartnerConnectionRepositoryMock = new Mock<IBankPartnerConnectionRepository>();
		bankPartnerConnectionRepositoryMock.Setup(connection =>
				connection.GetActiveAsync(_verifyRequest.RtgsGlobalId, _verifyRequest.Alias,
					It.IsAny<CancellationToken>()))
			.ReturnsAsync(new BankPartnerConnection { ConnectionId = "connection-id-1" });

		_jsonSignaturesClientMock
			.Setup(client => client.VerifyPrivateSignatureAsync(
				_verifyRequest.Message,
				_verifyRequest.PrivateSignature,
				"connection-id-1",
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(true)
			.Verifiable();

		var logger = new FakeLogger<MessageController>();

		_controller = new MessageController(
			logger,
			_jsonSignaturesClientMock.Object,
			bankPartnerConnectionRepositoryMock.Object,
			Mock.Of<IRtgsConnectionRepository>(),
			Mock.Of<IWalletClient>());
	}

	public async Task InitializeAsync() =>
		_response = await _controller.Verify(_verifyRequest);

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPostingVerifyPrivateSignatureRequest_ThenCallVerifyPrivateSignatureWithExpected() =>
		_jsonSignaturesClientMock.Verify();

	[Fact]
	public void WhenPostingVerifyPrivateSignatureRequest_ThenReturnOkResponseWithVerifiedTrue()
	{
		var verifyPrivateSignatureResponse = new VerifyResponse
		{
			Verified = true
		};

		_response.Should().BeOfType<OkObjectResult>()
			.Which.Value.Should().BeEquivalentTo(verifyPrivateSignatureResponse);
	}
}
