
using Booker.Api.Data;

public sealed class BookStats
{
    public required Guid Id { get; set; }

    public required string BookIsbn { get; set; }

    public Book Book { get; set; } = null!;

    public required int AvgLoanTime { get; set; }

    public required int BorrowedCount { get; set; }
}
