using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.Repositories;

public interface IRtgsConnectionRepository
{
	Task CreateAsync(RtgsConnection rtgsConnection, CancellationToken cancellationToken = default);
}
