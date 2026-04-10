using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Transaction> Transactions { get; }
    DbSet<User> Users { get; }
    DbSet<Category> Categories { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
