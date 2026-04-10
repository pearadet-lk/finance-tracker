using FinanceTracker.Application.Commands.Transactions;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FinanceTracker.UnitTests;

public class CreateTransactionHandlerTests
{
    private readonly Mock<IRedisService> _redisMock = new();

    private IAppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewGuid()
    {
        var context = CreateInMemoryContext();
        _redisMock.Setup(r => r.RemoveByPatternAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _redisMock.Setup(r => r.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        var handler = new CreateTransactionHandler(context, _redisMock.Object);

        var command = new CreateTransactionCommand(
            UserId: Guid.NewGuid(),
            Amount: 250.00m,
            Type: "Expense",
            CategoryId: Guid.NewGuid(),
            Description: "Test transaction",
            Date: DateTime.UtcNow);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsTransaction()
    {
        var context = CreateInMemoryContext();
        _redisMock.Setup(r => r.RemoveByPatternAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _redisMock.Setup(r => r.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        var handler = new CreateTransactionHandler(context, _redisMock.Object);
        var userId = Guid.NewGuid();

        await handler.Handle(new CreateTransactionCommand(
            userId, 100m, "Income", Guid.NewGuid(), "Salary", DateTime.UtcNow),
            CancellationToken.None);

        var saved = await context.Transactions.FirstOrDefaultAsync(t => t.UserId == userId);
        Assert.NotNull(saved);
        Assert.Equal(100m, saved.Amount);
        Assert.Equal(TransactionType.Income, saved.Type);
    }
}

// Test DbContext helper
public class TestDbContext : DbContext, IAppDbContext
{
    public TestDbContext(DbContextOptions options) : base(options) { }
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
}
