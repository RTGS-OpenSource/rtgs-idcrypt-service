using Moq;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.Service.Controllers;
using RTGS.Service.Dtos;
using Xunit;

namespace RTGS.Service.Tests.Controllers.SignMessageControllerTests;

public class SignMessageControllerTests
{
	public SignMessageControllerTests()
	{

	}

	[Fact]
	public void WhenPostingSignMessageRequest_ThenSignMessageIsCalledWithExpected()
	{
		var jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();

		var controller = new SignMessageController(jsonSignaturesClientMock.Object);

		var signMessageRequest = new SignMessageRequest();

		controller.Post(signMessageRequest);
	}
}
