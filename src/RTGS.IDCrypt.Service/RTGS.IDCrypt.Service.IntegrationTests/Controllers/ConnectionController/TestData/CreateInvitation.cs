﻿using System.Text.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;
using RTGS.IDCryptSDK.Connections.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.TestData;

public class CreateInvitation
{
	public const string Path = "/connections/create-invitation";

	public static CreateInvitationResponse Response => new()
	{
		ConnectionId = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
		Invitation = new ConnectionInvitation
		{
			Id = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
			Type = "https://didcomm.org/my-family/1.0/my-message-type",
			Label = "Bob",
			RecipientKeys = new[]
			{
				"H3C2AVvLMv6gmMNam3uVAjZpfkcJCwDwnZn6z3wXmqPV"
			},
			ServiceEndpoint = "http://192.168.56.101:8020"
		}
	};

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, JsonSerializer.Serialize(Response));
}
