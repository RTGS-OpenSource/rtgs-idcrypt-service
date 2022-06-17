namespace RTGS.IDCrypt.Service.Models.ConnectionInvitations;

public abstract class ConnectionInvitationBase
{
	public string Id { get; init; }
	public string Type { get; init; }
	public string Alias { get; init; }
	public string Label { get; init; }
	public string[] RecipientKeys { get; init; }
	public string ServiceEndpoint { get; init; }
	public string PublicDid { get; init; }
	public string ImageUrl { get; init; }
	public string Did { get; init; }
	public string InvitationUrl { get; init; }
	public string FromRtgsGlobalId { get; init; }
}
