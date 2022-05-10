using RTGS.IDCrypt.Service.IntegrationTests.Helpers;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.TestData;

public static class ReceiveInvitation
{
	public const string Path = "/connections/receive-invitation";

	private static string SerialisedResponse => $@"{{ 
		""accept"": ""accept"",
		""alias"": ""alias"",
		""connection_id"": ""connection-id"",
		""connection_protocol"": ""connection-protocol"",
		""created_at"": ""created-at"",
		""invitation_key"": ""invitation-key"",
		""invitation_msg_id"": ""invitation-message-id"",
		""invitation_mode"": ""invitation-mode"",
		""my_did"": ""my-did"",
		""request_id"": ""request-id"",
		""rfc23_state"": ""rfc-23-state"",
		""routing_state"": ""routing-state"",
		""state"": ""state"",
		""their_did"": ""their-did"",
		""their_label"": ""their-label"",
		""their_role"": ""their-role"",
		""updated_at"": ""updated-at""
	}}";

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, SerialisedResponse);
}


