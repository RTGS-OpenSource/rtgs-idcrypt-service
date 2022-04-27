namespace RTGS.Service.Dtos
{
	public record SignMessageResponse
	{
		public string PublicDidSignature { get; init; }
		public string PairwiseDidSignature { get; init; }
	}
}
