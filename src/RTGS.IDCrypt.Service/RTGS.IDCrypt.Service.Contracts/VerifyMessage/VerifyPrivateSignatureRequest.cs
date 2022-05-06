namespace RTGS.IDCrypt.Service.Contracts.VerifyMessage
{
	public record VerifyPrivateSignatureRequest
	{
		public string RtgsGlobalId { get; init; }
		public string Message { get; init; }
		public string PrivateSignature { get; init; }
		public string Alias { get; init; }
	}
}
