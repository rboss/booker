namespace Booker.Api.Data;

public sealed class BookEntity
{
    public Guid Id { get; set; }

    public required string BookIsbn { get; set; }

    public Book Book { get; set; } = null!;

    public string? LoanedToMemberId { get; set; }
}