﻿namespace RTGS.IDCrypt.Service.Contracts.SignMessage;

public record SignMessageResponse
{
	public string PublicDidSignature { get; init; }
	public string PairwiseDidSignature { get; init; }
	public string Alias { get; init; }
}