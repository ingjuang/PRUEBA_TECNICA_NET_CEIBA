using EventosVivosBackNet.Application.Commond.Interfaces.Repositories;
using EventosVivosBackNet.Application.Commond.Interfaces.Repositories.DbEventosVivos;
using EventosVivosBackNet.Infrastructure.Context;
using EventosVivosBackNet.Infrastructure.Repositories.DbEventosVivos;

namespace EventosVivosBackNet.Infrastructure.Repositories
{
    internal class UnitOfWorkEventosVivos : IUnitOfWorkDbEventosVivos
    {
        private readonly AppDbContext _context;
        private IDbEventosVivosRepository? _repository;

        public UnitOfWorkEventosVivos(AppDbContext context)
        {
            _context = context;
        }

        public IDbEventosVivosRepository Repository =>
            _repository ??= new DbEventosVivosRepository(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
