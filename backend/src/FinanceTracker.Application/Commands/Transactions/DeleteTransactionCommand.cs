using FinanceTracker.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Commands.Transactions;

public record DeleteTransactionCommand(Guid Id, Guid UserId) : IRequest<bool>;

public class DeleteTransactionHandler : IRequestHandler<DeleteTransactionCommand, bool>
{
    private readonly IAppDbContext _context;
    private readonly IRedisService _redis;

    public DeleteTransactionHandler(IAppDbContext context, IRedisService redis)
    {
        _context = context;
        _redis = redis;
    }

    public async Task<bool> Handle(DeleteTransactionCommand request, CancellationToken ct)
    {
        var entity = await _context.Transactions
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId, ct);

        if (entity is null) return false;

        // Soft delete
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        // Invalidate cache
        await _redis.RemoveByPatternAsync($"transactions:{request.UserId}:*");
        await _redis.RemoveAsync($"dashboard:{request.UserId}");

        return true;
    }
}
