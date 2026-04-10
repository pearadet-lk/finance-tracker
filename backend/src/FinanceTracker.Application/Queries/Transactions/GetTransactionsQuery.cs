using FinanceTracker.Application.Common;
using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Queries.Transactions;

public record GetTransactionsQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20,
    string? Type = null,
    Guid? CategoryId = null,
    DateTime? From = null,
    DateTime? To = null
) : IRequest<PagedResult<TransactionDto>>;

public class GetTransactionsHandler
    : IRequestHandler<GetTransactionsQuery, PagedResult<TransactionDto>>
{
    private readonly IAppDbContext _context;
    private readonly IRedisService _redis;

    public GetTransactionsHandler(IAppDbContext context, IRedisService redis)
    {
        _context = context;
        _redis = redis;
    }

    public async Task<PagedResult<TransactionDto>> Handle(
        GetTransactionsQuery request, CancellationToken ct)
    {
        var query = _context.Transactions
            .Where(x => x.UserId == request.UserId)
            .Include(x => x.Category)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Type))
            query = query.Where(x => x.Type.ToString() == request.Type);

        if (request.CategoryId.HasValue)
            query = query.Where(x => x.CategoryId == request.CategoryId);

        if (request.From.HasValue)
            query = query.Where(x => x.Date >= request.From);

        if (request.To.HasValue)
            query = query.Where(x => x.Date <= request.To);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.Date)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new TransactionDto(
                x.Id,
                x.Amount,
                x.Type.ToString(),
                x.Category.Name,
                x.Category.Color,
                x.Category.Icon,
                x.Description,
                x.Date,
                x.CreatedAt))
            .ToListAsync(ct);

        var result = new PagedResult<TransactionDto>(items, total, request.Page, request.PageSize);

        return result;
    }
}
