namespace RTGS.IDCrypt.Service.Helpers;

public class DateTimeProvider : IDateTimeProvider
{
	public DateTime UtcNow => DateTime.UtcNow;
}
