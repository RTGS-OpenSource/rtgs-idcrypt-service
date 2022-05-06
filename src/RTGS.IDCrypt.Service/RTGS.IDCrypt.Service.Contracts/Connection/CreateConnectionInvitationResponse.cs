namespace RTGS.IDCrypt.Service.Contracts.Connection;

/// <summary>
/// Model of the response from /connections/create-invitation.
/// </summary>
public record CreateConnectionInvitationResponse
{
	/// <summary>
	/// Connection identifier.
	/// </summary>
	public string ConnectionId { get; init; }

	/// <summary>
	/// Invitation URL.
	/// </summary>
	public string InvitationUrl { get; init; }

	/// <summary>
	/// The invitation.
	/// </summary>
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
	public string Id { get; init; }

	/// <summary>
	/// Message type.
	/// </summary>
	public string Type { get; init; }

	/// <summary>
	/// DID for connection invitation.
	/// </summary>
	public string Did { get; init; }

	/// <summary>
	/// Optional label for connection invitation.
	/// </summary>
	public string Label { get; init; }

	/// <summary>
	/// Optional image URL for connection invitation.
	/// </summary>
	public string ImageUrl { get; init; }

	/// <summary>
	/// List of recipient keys.
	/// </summary>
	public string[] RecipientKeys { get; init; }

	/// <summary>
	/// Service endpoint at which to reach this agent.
	/// </summary>
	public string ServiceEndpoint { get; init; }
}
