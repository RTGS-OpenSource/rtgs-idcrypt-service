namespace RTGS.IDCrypt.Service.Contracts.VerifyMessage
{
	public record VerifyPrivateSignatureResponse
	{
		public bool Verified { get; init; }
	}
}
