using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global soft delete filter
        modelBuilder.Entity<Transaction>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Category>().HasQueryFilter(x => !x.IsDeleted);

        // Transaction configuration
        modelBuilder.Entity<Transaction>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.Type).HasConversion<string>();
            e.HasOne(x => x.User).WithMany(u => u.Transactions).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Category).WithMany(c => c.Transactions).HasForeignKey(x => x.CategoryId);
        });

        // Category configuration
        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).HasConversion<string>();
        });

        // Seed categories
        var categories = new List<Category>
        {
            new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000001"), Name = "Salary", Type = Domain.Enums.TransactionType.Income, Icon = "💼", Color = "#10b981" },
            new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000002"), Name = "Freelance", Type = Domain.Enums.TransactionType.Income, Icon = "💻", Color = "#3b82f6" },
            new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000003"), Name = "Food & Dining", Type = Domain.Enums.TransactionType.Expense, Icon = "🍽️", Color = "#f59e0b" },
            new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000004"), Name = "Transport", Type = Domain.Enums.TransactionType.Expense, Icon = "🚗", Color = "#8b5cf6" },
            new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000005"), Name = "Shopping", Type = Domain.Enums.TransactionType.Expense, Icon = "🛍️", Color = "#ec4899" },
            new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000006"), Name = "Utilities", Type = Domain.Enums.TransactionType.Expense, Icon = "⚡", Color = "#ef4444" },
            new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000007"), Name = "Healthcare", Type = Domain.Enums.TransactionType.Expense, Icon = "🏥", Color = "#06b6d4" },
            new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000008"), Name = "Entertainment", Type = Domain.Enums.TransactionType.Expense, Icon = "🎮", Color = "#f97316" },
        };
        modelBuilder.Entity<Category>().HasData(categories);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
