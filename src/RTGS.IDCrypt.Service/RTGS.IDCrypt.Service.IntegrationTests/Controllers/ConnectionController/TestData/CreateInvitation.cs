using System.Text.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;
using RTGS.IDCryptSDK.Connections.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.TestData;

public static class CreateInvitation
{
	public const string Path = "/connections/create-invitation";

	public static CreateConnectionInvitationResponse Response => new()
	{
		ConnectionId = "connection-id",
		Alias = "alias",
		Invitation = new ConnectionInvitation
		{
			Id = "id",
			Type = "type",
			Label = "label",
			RecipientKeys = new[]
			{
				"recipient-key-1"
			},
			ServiceEndpoint = "service-endpoint"
		}
	};

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, JsonSerializer.Serialize(Response));
}
