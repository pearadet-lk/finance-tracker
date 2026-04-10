using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Queries.Transactions;

public record GetDashboardQuery(Guid UserId, int Year) : IRequest<DashboardDto>;

public class GetDashboardHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IAppDbContext _context;
    private readonly IRedisService _redis;

    public GetDashboardHandler(IAppDbContext context, IRedisService redis)
    {
        _context = context;
        _redis = redis;
    }

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken ct)
    {
        var cacheKey = $"dashboard:{request.UserId}:{request.Year}";

        var cached = await _redis.GetAsync<DashboardDto>(cacheKey);
        if (cached is not null) return cached;

        var transactions = await _context.Transactions
            .Where(x => x.UserId == request.UserId && x.Date.Year == request.Year)
            .Include(x => x.Category)
            .ToListAsync(ct);

        var totalIncome = transactions
            .Where(x => x.Type == Domain.Enums.TransactionType.Income)
            .Sum(x => x.Amount);

        var totalExpense = transactions
            .Where(x => x.Type == Domain.Enums.TransactionType.Expense)
            .Sum(x => x.Amount);

        var monthlyTrend = transactions
            .GroupBy(x => x.Date.Month)
            .Select(g => new MonthlyTrendDto(
                new DateTime(request.Year, g.Key, 1).ToString("MMM"),
                g.Where(t => t.Type == Domain.Enums.TransactionType.Income).Sum(t => t.Amount),
                g.Where(t => t.Type == Domain.Enums.TransactionType.Expense).Sum(t => t.Amount)))
            .OrderBy(x => x.Month)
            .ToList();

        var categoryBreakdown = transactions
            .Where(x => x.Type == Domain.Enums.TransactionType.Expense)
            .GroupBy(x => new { x.Category.Name, x.Category.Color })
            .Select(g => new CategoryBreakdownDto(g.Key.Name, g.Key.Color, g.Sum(x => x.Amount), g.Count()))
            .OrderByDescending(x => x.Amount)
            .Take(6)
            .ToList();

        var result = new DashboardDto(totalIncome, totalExpense, totalIncome - totalExpense, monthlyTrend, categoryBreakdown);

        await _redis.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));

        return result;
    }
}
