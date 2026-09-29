namespace Booker.Api.Models;

/// <summary>Represents a book and its physical copies.</summary>
public sealed record BookResponse(
    string Isbn,
    string Title,
    string Author,
    IReadOnlyList<BookCopyResponse> Copies);

/// <summary>Represents the loan status of one physical book copy.</summary>
public sealed record BookCopyResponse(
    Guid Id,
    string? LoanedToMemberId);

/// <summary>Represents one loan of a book copy, from borrowing to return.</summary>
public sealed record LoanHistoryResponse(
    Guid Id,
    Guid BookCopyId,
    string Isbn,
    string Title,
    string MemberId,
    DateTimeOffset BorrowedAt,
    DateTimeOffset? ReturnedAt);

public sealed record BookStatsResponse(
    int AvgLoanTime,
    int BorrowedCount);


/// <summary>Identifies the member borrowing or returning a book.</summary>
public sealed record MemberRequest
{
    /// <summary>The member identifier associated with the loan.</summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required string MemberId { get; init; }
}