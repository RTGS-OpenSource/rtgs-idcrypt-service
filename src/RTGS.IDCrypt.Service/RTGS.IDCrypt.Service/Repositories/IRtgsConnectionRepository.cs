using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.Repositories;

public interface IRtgsConnectionRepository
{
	Task ActivateAsync(string connectionId, CancellationToken cancellationToken = default);
	Task CreateAsync(RtgsConnection rtgsConnection, CancellationToken cancellationToken = default);
}
