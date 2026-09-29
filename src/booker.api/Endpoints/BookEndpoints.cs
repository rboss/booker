using System.Linq.Expressions;
using Booker.Api.Data;
using Booker.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Booker.Api.Endpoints;

public static class BookEndpoints
{
    public static IEndpointRouteBuilder MapBookEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var books = endpoints.MapGroup("/books").WithTags("Books");

        books.MapGet("/", async (BookDbContext database, CancellationToken cancellationToken) =>
            (IResult)TypedResults.Ok((await database.Books
                .AsNoTracking()
                .Include(book => book.Copies)
                .OrderBy(book => book.Title)
                .ToListAsync(cancellationToken))
                .Select(ToResponse)
                .ToList()))
            .WithName("ListBooks");

        books.MapGet("/{isbn}", async Task<Results<Ok<BookResponse>, NotFound>> (
            string isbn,
            BookDbContext database,
            CancellationToken cancellationToken) =>
        {
            var book = await GetBook(database, cancellationToken, isbn);
            return book is null
                ? TypedResults.NotFound()
                : TypedResults.Ok(ToResponse(book));
        }).WithName("GetBook");

        books.MapGet("/{isbn}/history", async Task<Results<Ok<List<LoanHistoryResponse>>, NotFound>> (
            string isbn,
            BookDbContext database,
            CancellationToken cancellationToken) =>
        {
            if (!await database.Books.AnyAsync(book => book.Isbn == isbn, cancellationToken))
            {
                return TypedResults.NotFound();
            }

            var history = await database.LoanHistory
                .AsNoTracking()
                .Where(loan => loan.BookCopy.BookIsbn == isbn)
                .OrderByDescending(loan => loan.BorrowedAt)
                .Select(ToHistoryResponse)
                .ToListAsync(cancellationToken);
            return TypedResults.Ok(history);
        }).WithName("GetBookLoanHistory");

        books.MapGet("/{isbn}/stats", async Task<Results<Ok<BookStatsResponse>, NotFound>> (
            string isbn,
            BookDbContext database,
            CancellationToken cancellationToken) =>
        {
            if (!await database.Books.AnyAsync(book => book.Isbn == isbn, cancellationToken))
            {
                return TypedResults.NotFound();
            }

            var stats = await database.BookStats
                .AsNoTracking()
                .Where(stat => stat.BookIsbn == isbn)
                .ToListAsync(cancellationToken);

            // A book can have several stats rows; weight each average by how often it was borrowed.
            var borrowedCount = stats.Sum(stat => stat.BorrowedCount);
            var avgLoanTime = borrowedCount == 0
                ? 0
                : (int)Math.Round((double)stats.Sum(stat => stat.AvgLoanTime * stat.BorrowedCount) / borrowedCount);
            return TypedResults.Ok(new BookStatsResponse(avgLoanTime, borrowedCount));
        }).WithName("GetBookStats");

        books.MapPost("/copies/{isbn}/loan", async Task<Results<Ok<BookResponse>, NotFound, Conflict<string>>> (
            string isbn,
            MemberRequest request,
            BookDbContext database,
            CancellationToken cancellationToken) =>
        {
            var copies = await database.BookEntities.Where(copy => copy.BookIsbn == isbn).ToListAsync(cancellationToken);

            if (copies.FirstOrDefault(c => c.LoanedToMemberId == request.MemberId) != null)
            {
                return TypedResults.Conflict("Already loaned by member");
            }

            var copyToLoan = copies.FirstOrDefault(c => c.LoanedToMemberId == null);

            if (copyToLoan == null)
            {
                return TypedResults.Conflict("No copies available");
            }

            copyToLoan.LoanedToMemberId = request.MemberId;
            database.LoanHistory.Add(new LoanHistory
            {
                BookCopyId = copyToLoan.Id,
                MemberId = request.MemberId,
                BorrowedAt = DateTimeOffset.UtcNow,
            });
            await database.SaveChangesAsync(cancellationToken);

   
            var book = await GetBook(database, cancellationToken, isbn);
            if (book is null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(ToResponse(book));
        }).WithName("LoanBook");

        books.MapPost("/copies/{isbn}/return", async Task<Results<Ok<BookResponse>, NotFound, Conflict<string>>> (
            string isbn,
            MemberRequest request,
            BookDbContext database,
            CancellationToken cancellationToken) =>
        {
            var copy = await database.BookEntities.FirstOrDefaultAsync(copy => copy.Book.Isbn == isbn && copy.LoanedToMemberId == request.MemberId, cancellationToken);

            if (copy is null)
            {
                return TypedResults.Conflict("Book copy is not currently loaned.");
            }

            copy.LoanedToMemberId = null;
            var openLoan = await database.LoanHistory
                .Where(loan => loan.BookCopyId == copy.Id && loan.MemberId == request.MemberId && loan.ReturnedAt == null)
                .OrderByDescending(loan => loan.BorrowedAt)
                .FirstOrDefaultAsync(cancellationToken);
            if (openLoan is not null)
            {
                openLoan.ReturnedAt = DateTimeOffset.UtcNow;
            }

            await database.SaveChangesAsync(cancellationToken);

            var book = await GetBook(database, cancellationToken, isbn);
            if (book is null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(ToResponse(book));
        }).WithName("ReturnBook");

        var members = endpoints.MapGroup("/members").WithTags("Members");

        members.MapGet("/{memberId}/history", async (
            string memberId,
            BookDbContext database,
            CancellationToken cancellationToken) =>
            TypedResults.Ok(await database.LoanHistory
                .AsNoTracking()
                .Where(loan => loan.MemberId == memberId)
                .OrderByDescending(loan => loan.BorrowedAt)
                .Select(ToHistoryResponse)
                .ToListAsync(cancellationToken)))
            .WithName("GetMemberLoanHistory");

        return endpoints;
    }

    private static Task<Book?> GetBook(BookDbContext database, CancellationToken cancellationToken, string isbn)
    {
        return database.Books
               .AsNoTracking()
               .Include(book => book.Copies)
               .SingleOrDefaultAsync(book => book.Isbn == isbn, cancellationToken);
    }

    private static readonly Expression<Func<LoanHistory, LoanHistoryResponse>> ToHistoryResponse = loan =>
        new(
            loan.Id,
            loan.BookCopyId,
            loan.BookCopy.BookIsbn,
            loan.BookCopy.Book.Title,
            loan.MemberId,
            loan.BorrowedAt,
            loan.ReturnedAt);

    private static BookResponse ToResponse(Book book) =>
        new(
            book.Isbn,
            book.Title,
            book.Author,
            book.Copies
                .Select(copy => new BookCopyResponse(copy.Id, copy.LoanedToMemberId))
                .ToList());
}