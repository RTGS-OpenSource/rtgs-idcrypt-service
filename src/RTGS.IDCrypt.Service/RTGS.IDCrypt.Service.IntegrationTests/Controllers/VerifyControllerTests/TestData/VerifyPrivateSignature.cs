using RTGS.IDCrypt.Service.Contracts.VerifyMessage;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;
using RTGS.IDCryptSDK.Connections.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.VerifyControllerTests.TestData;

public class VerifyPrivateSignature
{
	public const string ConnectionsPath = "/connections";
	public const string VerifyPrivateSignaturePath = "/json-signatures/verify/connection-did";

	public static ConnectionResponse ExpectedConnectionsResponse => new()
	{
		Accept = "accept",
		Alias = "alias",
		ConnectionId = "connection-id",
		ConnectionProtocol = "connection-protocol",
		CreatedAt = "created-at",
		InvitationKey = "invitation-key",
		InvitationMessageId = "invitation-message-id",
		InvitationMode = "invitation-mode",
		MyDid = "my-did",
		RequestId = "request-id",
		Rfc23State = "rfc-23-state",
		RoutingState = "routing-state",
		State = "active",
		TheirDid = "their-did",
		TheirLabel = "their-label",
		TheirRole = "their-role",
		UpdatedAt = "updated-at"
	};

	public static VerifyPrivateSignatureResponse ExpectedVerifyResponse => new()
	{
		Verified = true
	};

	private static string SerialisedVerifiedResponse =>
		$@"{{
			""verified"": {ExpectedVerifyResponse.Verified.ToString().ToLowerInvariant()}
		}}";

	private static string SerialisedConnectionsResponse =>
		$@"{{ 
			""results"": [
				{{
					""accept"": ""{ExpectedConnectionsResponse.Accept}"",
					""alias"": ""{ExpectedConnectionsResponse.Alias}"",
					""connection_id"": ""{ExpectedConnectionsResponse.ConnectionId}"",
					""connection_protocol"": ""{ExpectedConnectionsResponse.ConnectionProtocol}"",
					""created_at"": ""{ExpectedConnectionsResponse.CreatedAt}"",
					""invitation_key"": ""{ExpectedConnectionsResponse.InvitationKey}"",
					""invitation_msg_id"": ""{ExpectedConnectionsResponse.InvitationMessageId}"",
					""invitation_mode"": ""{ExpectedConnectionsResponse.InvitationMode}"",
					""my_did"": ""{ExpectedConnectionsResponse.MyDid}"",
					""request_id"": ""{ExpectedConnectionsResponse.RequestId}"",
					""rfc23_state"": ""{ExpectedConnectionsResponse.Rfc23State}"",
					""routing_state"": ""{ExpectedConnectionsResponse.RoutingState}"",
					""state"": ""{ExpectedConnectionsResponse.State}"",
					""their_did"": ""{ExpectedConnectionsResponse.TheirDid}"",
					""their_label"": ""{ExpectedConnectionsResponse.TheirLabel}"",
					""their_role"": ""{ExpectedConnectionsResponse.TheirRole}"",
					""updated_at"": ""{ExpectedConnectionsResponse.UpdatedAt}""
				}}
			]
		}}";

	public static HttpRequestResponseContext ConnectionsHttpRequestResponseContext =>
		new(ConnectionsPath, SerialisedConnectionsResponse);
	public static HttpRequestResponseContext VerifyHttpRequestResponseContext =>
		new(VerifyPrivateSignaturePath, SerialisedVerifiedResponse);
}
