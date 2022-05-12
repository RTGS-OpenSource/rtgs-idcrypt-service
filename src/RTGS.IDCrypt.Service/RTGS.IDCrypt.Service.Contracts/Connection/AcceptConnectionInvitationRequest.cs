namespace RTGS.IDCrypt.Service.Contracts.Connection;

/// <summary>
/// Model of the invitation to accept.
/// </summary>
/// <param name="Id">Message identifier.</param>
/// <param name="Type">Message type.</param>
/// <param name="Alias">Alias for the connection.</param>
/// <param name="Label">Label for connection invitation.</param>
/// <param name="RecipientKeys">List of recipient keys.</param>
/// <param name="ServiceEndpoint">Service endpoint at which to reach this agent.</param>
public record AcceptConnectionInvitationRequest(
	string Id,
	string Type,
	string Alias,
	string Label,
	string[] RecipientKeys,
	string ServiceEndpoint);
