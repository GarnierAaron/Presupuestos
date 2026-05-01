using Microsoft.EntityFrameworkCore;
using Presupuestos.Application.Abstractions;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Infrastructure.Persistence;

public class AppConfigRepository : IAppConfigRepository
{
    private readonly AppDbContext _db;

    public AppConfigRepository(AppDbContext db) => _db = db;

    public async Task<AppConfig?> GetSingletonAsync(CancellationToken cancellationToken = default) =>
        await _db.AppConfigs.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
}
