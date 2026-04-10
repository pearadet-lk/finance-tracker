using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using MediatR;

namespace FinanceTracker.Application.Commands.Transactions;

// Command
public record CreateTransactionCommand(
    Guid UserId,
    decimal Amount,
    string Type,
    Guid CategoryId,
    string Description,
    DateTime Date
) : IRequest<Guid>;

// Handler
public class CreateTransactionHandler : IRequestHandler<CreateTransactionCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly IRedisService _redis;

    public CreateTransactionHandler(IAppDbContext context, IRedisService redis)
    {
        _context = context;
        _redis = redis;
    }

    public async Task<Guid> Handle(CreateTransactionCommand request, CancellationToken ct)
    {
        var normalizedDate = request.Date.Kind switch
        {
            DateTimeKind.Utc => request.Date,
            DateTimeKind.Local => request.Date.ToUniversalTime(),
            _ => DateTime.SpecifyKind(request.Date, DateTimeKind.Utc)
        };

        var entity = new Transaction
        {
            UserId = request.UserId,
            Amount = request.Amount,
            Type = Enum.Parse<TransactionType>(request.Type, true),
            CategoryId = request.CategoryId,
            Description = request.Description,
            Date = normalizedDate
        };

        _context.Transactions.Add(entity);
        await _context.SaveChangesAsync(ct);

        // Invalidate cache
        await _redis.RemoveByPatternAsync($"transactions:{request.UserId}:*");
        await _redis.RemoveAsync($"dashboard:{request.UserId}");

        return entity.Id;
    }
}
