using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Domain.Entities;

public class Transaction : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public Guid CategoryId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
