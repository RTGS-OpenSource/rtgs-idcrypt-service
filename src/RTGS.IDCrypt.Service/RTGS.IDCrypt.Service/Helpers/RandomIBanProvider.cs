namespace RTGS.IDCrypt.Service.Helpers;

public class RandomIBanProvider : IIBanProvider
{
	public string Generate()
	{
		Random random = new();
		double tt = random.Next() * 8999999;
		var ktnr = Math.Round(tt) + 1000000;
		var pruef = (ktnr * 1000000) + 43;
		var pruef2 = pruef % 97;
		pruef = 98 - pruef2;
		if (pruef > 9)
		{
			return "DE" + pruef + "70050000" + "000" + ktnr;
		}

		return "DE0" + pruef + "70050000" + "000" + ktnr;
	}
}
