using System.Text.Json.Serialization;

namespace RTGS.IDCrypt.Service.Contracts.Connection;

/// <summary>
/// Model of the request for a bank invitation.
/// </summary>
public record CreateConnectionInvitationForBankRequest
{
	/// <summary>
	/// The RTGS.Global ID of the bank to invite.
	/// </summary>
	[JsonPropertyName("rtgsGlobalId")]
	public string RtgsGlobalId { get; init; }
}
