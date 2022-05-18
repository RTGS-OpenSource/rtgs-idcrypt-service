using System.Text.Json.Serialization;

namespace RTGS.IDCrypt.Service.Contracts.Connection;

/// <summary>
/// Model of the response from /connections.
/// </summary>
public record CreateConnectionInvitationResponse
{
	/// <summary>
	/// Connection identifier.
	/// </summary>
	[JsonPropertyName("connectionId")]
	public string ConnectionId { get; init; }

	/// <summary>
	/// Connection alias.
	/// </summary>
	[JsonPropertyName("alias")]
	public string Alias { get; init; }

	/// <summary>
	/// Public DID of the agent which created this invitation.
	/// </summary>
	[JsonPropertyName("agentPublicDid")]
	public string AgentPublicDid { get; init; }

	/// <summary>
	/// Invitation URL.
	/// </summary>
	[JsonPropertyName("invitationUrl")]
	public string InvitationUrl { get; init; }

	/// <summary>
	/// The invitation.
	/// </summary>
	[JsonPropertyName("invitation")]
	public ConnectionInvitation Invitation { get; init; }
}

/// <summary>
/// Model of the invitation.
/// </summary>
public record ConnectionInvitation
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
	/// DID for connection invitation.
	/// </summary>
	[JsonPropertyName("did")]
	public string Did { get; init; }

	/// <summary>
	/// Optional label for connection invitation.
	/// </summary>
	[JsonPropertyName("label")]
	public string Label { get; init; }

	/// <summary>
	/// Optional image URL for connection invitation.
	/// </summary>
	[JsonPropertyName("imageUrl")]
	public string ImageUrl { get; init; }

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
}
