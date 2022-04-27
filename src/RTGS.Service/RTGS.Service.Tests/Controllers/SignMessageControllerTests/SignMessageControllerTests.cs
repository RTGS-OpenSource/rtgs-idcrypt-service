using System.Threading.Tasks;
using Moq;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.Service.Controllers;
using RTGS.Service.Dtos;
using RTGS.Service.Storage;
using Xunit;

namespace RTGS.Service.Tests.Controllers.SignMessageControllerTests;

public class SignMessageControllerTests
{
	public SignMessageControllerTests()
	{

	}

	[Fact]
	public async Task WhenPostingSignMessageRequest_ThenSignMessageIsCalledWithExpected()
	{
		var jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();
		var storageTableResolver = new Mock<IStorageTableResolver>();

		var controller = new SignMessageController(
			storageTableResolver.Object,
			jsonSignaturesClientMock.Object);

		var signMessageRequest = new SignMessageRequest();

		await controller.Post(signMessageRequest);
	}
}
