using System.Text.Json.Serialization;

namespace RTGS.IDCrypt.Service.Contracts.Connection;

/// <summary>
/// Model of the request for an invitation.
/// </summary>
public record CreateConnectionInvitationRequest
{
	/// <summary>
	/// The RTGS.Global ID of the bank to invite.
	/// </summary>
	[JsonPropertyName("rtgsGlobalId")]
	public string RtgsGlobalId { get; init; }
}
