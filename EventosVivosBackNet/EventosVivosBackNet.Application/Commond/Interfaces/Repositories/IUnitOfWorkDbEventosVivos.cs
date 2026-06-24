using EventosVivosBackNet.Application.Commond.Interfaces.Repositories.DbEventosVivos;

namespace EventosVivosBackNet.Application.Commond.Interfaces.Repositories
{
    public interface IUnitOfWorkDbEventosVivos : IDisposable
    {
        IDbEventosVivosRepository Repository { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
