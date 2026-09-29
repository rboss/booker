namespace Booker.Api.Data;

public sealed class LoanHistory
{
    public Guid Id { get; set; }

    public Guid BookCopyId { get; set; }

    public BookEntity BookCopy { get; set; } = null!;

    public required string MemberId { get; set; }

    public DateTimeOffset BorrowedAt { get; set; }

    public DateTimeOffset? ReturnedAt { get; set; }
}
