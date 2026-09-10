using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
namespace CloudShopping.Infrastructure.Repositories
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException exception)
            {
                throw new InvalidOperationException("Registro alterado por outra operação. Recarregue antes de salvar.", exception);
            }
            catch (DbUpdateException exception) when (exception.InnerException is MySqlException { Number: 1062 })
            {
                throw new InvalidOperationException("Já existe um registro com estes dados únicos nesta loja.", exception);
            }
        }
    }
}
