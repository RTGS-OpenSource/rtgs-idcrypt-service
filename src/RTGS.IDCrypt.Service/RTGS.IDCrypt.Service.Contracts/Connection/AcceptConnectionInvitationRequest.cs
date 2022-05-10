namespace RTGS.IDCrypt.Service.Contracts.Connection;


/// <summary>
/// Model of the invitation to accept.
/// </summary>
public class AcceptConnectionInvitationRequest
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
	/// Alias for the connection.
	/// </summary>
	public string Alias { get; init; }

	/// <summary>
	/// Label for connection invitation.
	/// </summary>
	public string Label { get; init; }

	/// <summary>
	/// List of recipient keys.
	/// </summary>
	public string[] RecipientKeys { get; init; }

	/// <summary>
	/// Service endpoint at which to reach this agent.
	/// </summary>
	public string ServiceEndpoint { get; init; }
}
