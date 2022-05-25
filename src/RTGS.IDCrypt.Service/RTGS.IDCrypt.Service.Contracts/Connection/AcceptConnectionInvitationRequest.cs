using System.Text.Json.Serialization;

namespace RTGS.IDCrypt.Service.Contracts.Connection;

/// <summary>
/// Model of the invitation to accept.
/// </summary>
public class AcceptConnectionInvitationRequest
{
	/// <summary>
	/// Message identifier.
	/// </summary>
	[JsonPropertyName("id")]
	public string Id { get; init; }

	/// <summary>
	/// Message type.
	/// </summary>
	[JsonPropertyName("type")]
	public string Type { get; init; }

	/// <summary>
	/// Alias for the connection.
	/// </summary>
	[JsonPropertyName("alias")]
	public string Alias { get; init; }

	/// <summary>
	/// Label for connection invitation.
	/// </summary>
	[JsonPropertyName("label")]
	public string Label { get; init; }

	/// <summary>
	/// List of recipient keys.
	/// </summary>
	[JsonPropertyName("recipientKeys")]
	public string[] RecipientKeys { get; init; }

	/// <summary>
	/// Service endpoint at which to reach this agent.
	/// </summary>
	[JsonPropertyName("serviceEndpoint")]
	public string ServiceEndpoint { get; init; }

	/// <summary>
	/// Public DID of the agent which created this invitation.
	/// </summary>
	[JsonPropertyName("agentPublicDid")]
	public string AgentPublicDid { get; init; }

	/// <summary>
	/// The RTGS.Global ID of the inviting bank.
	/// </summary>
	[JsonPropertyName("rtgsGlobalId")]
	public string RtgsGlobalId { get; init; }
}
