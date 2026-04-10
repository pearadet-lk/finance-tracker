namespace FinanceTracker.Application.DTOs;

public record TransactionDto(
    Guid Id,
    decimal Amount,
    string Type,
    string CategoryName,
    string CategoryColor,
    string CategoryIcon,
    string Description,
    DateTime Date,
    DateTime CreatedAt
);

public record CategoryDto(
    Guid Id,
    string Name,
    string Type,
    string Icon,
    string Color
);

public record DashboardDto(
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balance,
    List<MonthlyTrendDto> MonthlyTrend,
    List<CategoryBreakdownDto> CategoryBreakdown
);

public record MonthlyTrendDto(string Month, decimal Income, decimal Expense);

public record CategoryBreakdownDto(string Category, string Color, decimal Amount, int Count);

public record AuthResponseDto(string Token, string RefreshToken, UserDto User);

public record UserDto(Guid Id, string Email, string FirstName, string LastName);
