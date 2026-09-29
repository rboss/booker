using System.Net;
using System.Net.Http.Json;
using Booker.Api.Data;
using Booker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace booker.Api.Tests;

public class BookEndpointsTests
{
    private const string Dune = "9780441172719";
    private const string Kindred = "9781472258229";
    private const string UnknownIsbn = "0000000000000";

    private BookApiFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new BookApiFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    // ListBooks

    [Test]
    public async Task ListBooks_ReturnsBooksOrderedByTitleWithCopies()
    {
        await _factory.SeedAsync(
            NewBook(Kindred, "Kindred"),
            NewBook(Dune, "Dune"),
            NewCopy(Dune),
            NewCopy(Dune, "ada"));

        var books = await GetJsonAsync<List<BookResponse>>("/books");

        Assert.That(books.Select(book => book.Title), Is.EqualTo(new[] { "Dune", "Kindred" }));
        Assert.That(books[0].Copies, Has.Count.EqualTo(2));
        Assert.That(books[0].Copies.Select(copy => copy.LoanedToMemberId), Is.EquivalentTo(new[] { null, "ada" }));
        Assert.That(books[1].Copies, Is.Empty);
    }

    // GetBook

    [Test]
    public async Task GetBook_ReturnsBook()
    {
        await _factory.SeedAsync(NewBook(Dune, "Dune"), NewCopy(Dune));

        var book = await GetJsonAsync<BookResponse>($"/books/{Dune}");

        Assert.That(book.Isbn, Is.EqualTo(Dune));
        Assert.That(book.Title, Is.EqualTo("Dune"));
        Assert.That(book.Copies, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetBook_UnknownIsbn_ReturnsNotFound()
    {
        await _factory.SeedAsync(NewBook(Dune, "Dune"));

        var response = await _client.GetAsync($"/books/{UnknownIsbn}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    // GetBookLoanHistory

    [Test]
    public async Task GetBookLoanHistory_ReturnsLoansOfThatBookNewestFirst()
    {
        var duneCopy = NewCopy(Dune);
        var kindredCopy = NewCopy(Kindred);
        await _factory.SeedAsync(
            NewBook(Dune, "Dune"),
            NewBook(Kindred, "Kindred"),
            duneCopy,
            kindredCopy,
            NewLoan(duneCopy, "ada", At(1), returnedAt: At(5)),
            NewLoan(duneCopy, "sam", At(10)),
            NewLoan(kindredCopy, "ada", At(3)));

        var history = await GetJsonAsync<List<LoanHistoryResponse>>($"/books/{Dune}/history");

        Assert.That(history.Select(loan => loan.MemberId), Is.EqualTo(new[] { "sam", "ada" }));
        Assert.That(history.Select(loan => loan.Title), Is.All.EqualTo("Dune"));
        Assert.That(history[0].ReturnedAt, Is.Null);
        Assert.That(history[1].ReturnedAt, Is.EqualTo(At(5)));
    }

    [Test]
    public async Task GetBookLoanHistory_UnknownIsbn_ReturnsNotFound()
    {
        await _factory.SeedAsync(NewBook(Dune, "Dune"));

        var response = await _client.GetAsync($"/books/{UnknownIsbn}/history");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    // GetBookStats

    [Test]
    public async Task GetBookStats_CombinesRowsWeightedByBorrowedCount()
    {
        await _factory.SeedAsync(
            NewBook(Dune, "Dune"),
            NewBook(Kindred, "Kindred"),
            NewStats(Dune, avgLoanTime: 10, borrowedCount: 1),
            NewStats(Dune, avgLoanTime: 20, borrowedCount: 3),
            NewStats(Kindred, avgLoanTime: 99, borrowedCount: 99));

        var stats = await GetJsonAsync<BookStatsResponse>($"/books/{Dune}/stats");

        // (10 * 1 + 20 * 3) / 4 = 17.5, rounded to even.
        Assert.That(stats, Is.EqualTo(new BookStatsResponse(AvgLoanTime: 18, BorrowedCount: 4)));
    }

    [Test]
    public async Task GetBookStats_NoStatsRows_ReturnsZeros()
    {
        await _factory.SeedAsync(NewBook(Dune, "Dune"));

        var stats = await GetJsonAsync<BookStatsResponse>($"/books/{Dune}/stats");

        Assert.That(stats, Is.EqualTo(new BookStatsResponse(AvgLoanTime: 0, BorrowedCount: 0)));
    }

    [Test]
    public async Task GetBookStats_UnknownIsbn_ReturnsNotFound()
    {
        await _factory.SeedAsync(NewBook(Dune, "Dune"));

        var response = await _client.GetAsync($"/books/{UnknownIsbn}/stats");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    // LoanBook

    [Test]
    public async Task LoanBook_LoansAFreeCopyAndOpensHistoryEntry()
    {
        var loaned = NewCopy(Dune, "sam");
        var free = NewCopy(Dune);
        await _factory.SeedAsync(NewBook(Dune, "Dune"), loaned, free);

        var response = await _client.PostAsJsonAsync($"/books/copies/{Dune}/loan", new { memberId = "ada" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var book = await ReadJsonAsync<BookResponse>(response);
        Assert.That(book.Copies.Single(copy => copy.Id == free.Id).LoanedToMemberId, Is.EqualTo("ada"));
        Assert.That(book.Copies.Single(copy => copy.Id == loaned.Id).LoanedToMemberId, Is.EqualTo("sam"));

        var history = await _factory.QueryAsync(database => database.LoanHistory.ToListAsync());
        Assert.That(history, Has.Count.EqualTo(1));
        Assert.That(history[0].BookCopyId, Is.EqualTo(free.Id));
        Assert.That(history[0].MemberId, Is.EqualTo("ada"));
        Assert.That(history[0].BorrowedAt, Is.EqualTo(DateTimeOffset.UtcNow).Within(TimeSpan.FromMinutes(1)));
        Assert.That(history[0].ReturnedAt, Is.Null);
    }

    [Test]
    public async Task LoanBook_MemberAlreadyHasACopy_ReturnsConflict()
    {
        await _factory.SeedAsync(NewBook(Dune, "Dune"), NewCopy(Dune, "ada"), NewCopy(Dune));

        var response = await _client.PostAsJsonAsync($"/books/copies/{Dune}/loan", new { memberId = "ada" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(await response.Content.ReadFromJsonAsync<string>(), Is.EqualTo("Already loaned by member"));
        Assert.That(await _factory.QueryAsync(database => database.LoanHistory.CountAsync()), Is.Zero);
    }

    [Test]
    public async Task LoanBook_NoFreeCopies_ReturnsConflict()
    {
        await _factory.SeedAsync(NewBook(Dune, "Dune"), NewCopy(Dune, "sam"));

        var response = await _client.PostAsJsonAsync($"/books/copies/{Dune}/loan", new { memberId = "ada" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(await response.Content.ReadFromJsonAsync<string>(), Is.EqualTo("No copies available"));
    }

    [Test]
    public async Task LoanBook_MissingMemberId_ReturnsBadRequest()
    {
        await _factory.SeedAsync(NewBook(Dune, "Dune"), NewCopy(Dune));

        var response = await _client.PostAsJsonAsync($"/books/copies/{Dune}/loan", new { });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    // ReturnBook

    [Test]
    public async Task ReturnBook_FreesMembersCopyAndClosesHistoryEntry()
    {
        var adasCopy = NewCopy(Dune, "ada");
        var samsCopy = NewCopy(Dune, "sam");
        var previousLoan = NewLoan(adasCopy, "ada", At(1), returnedAt: At(2));
        var openLoan = NewLoan(adasCopy, "ada", At(5));
        await _factory.SeedAsync(NewBook(Dune, "Dune"), adasCopy, samsCopy, previousLoan, openLoan);

        var response = await _client.PostAsJsonAsync($"/books/copies/{Dune}/return", new { memberId = "ada" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var book = await ReadJsonAsync<BookResponse>(response);
        Assert.That(book.Copies.Single(copy => copy.Id == adasCopy.Id).LoanedToMemberId, Is.Null);
        Assert.That(book.Copies.Single(copy => copy.Id == samsCopy.Id).LoanedToMemberId, Is.EqualTo("sam"));

        var history = await _factory.QueryAsync(database => database.LoanHistory.ToDictionaryAsync(loan => loan.Id));
        Assert.That(history[openLoan.Id].ReturnedAt, Is.EqualTo(DateTimeOffset.UtcNow).Within(TimeSpan.FromMinutes(1)));
        Assert.That(history[previousLoan.Id].ReturnedAt, Is.EqualTo(At(2)));
    }

    [Test]
    public async Task ReturnBook_MemberHasNoCopy_ReturnsConflict()
    {
        await _factory.SeedAsync(NewBook(Dune, "Dune"), NewCopy(Dune, "sam"));

        var response = await _client.PostAsJsonAsync($"/books/copies/{Dune}/return", new { memberId = "ada" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(
            await _factory.QueryAsync(database => database.BookEntities.SingleAsync()),
            Has.Property(nameof(BookEntity.LoanedToMemberId)).EqualTo("sam"));
    }

    // GetMemberLoanHistory

    [Test]
    public async Task GetMemberLoanHistory_ReturnsOnlyThatMembersLoansNewestFirst()
    {
        var duneCopy = NewCopy(Dune);
        var kindredCopy = NewCopy(Kindred);
        await _factory.SeedAsync(
            NewBook(Dune, "Dune"),
            NewBook(Kindred, "Kindred"),
            duneCopy,
            kindredCopy,
            NewLoan(duneCopy, "ada", At(1), returnedAt: At(4)),
            NewLoan(kindredCopy, "ada", At(6)),
            NewLoan(duneCopy, "sam", At(8)));

        var history = await GetJsonAsync<List<LoanHistoryResponse>>("/members/ada/history");

        Assert.That(history.Select(loan => loan.Title), Is.EqualTo(new[] { "Kindred", "Dune" }));
        Assert.That(history.Select(loan => loan.MemberId), Is.All.EqualTo("ada"));
        Assert.That(history[0].Isbn, Is.EqualTo(Kindred));
    }

    [Test]
    public async Task GetMemberLoanHistory_UnknownMember_ReturnsEmptyList()
    {
        await _factory.SeedAsync(NewBook(Dune, "Dune"));

        var history = await GetJsonAsync<List<LoanHistoryResponse>>("/members/nobody/history");

        Assert.That(history, Is.Empty);
    }

    private async Task<T> GetJsonAsync<T>(string url) =>
        await _client.GetFromJsonAsync<T>(url) ?? throw new AssertionException($"GET {url} returned an empty body.");

    private static async Task<T> ReadJsonAsync<T>(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<T>() ?? throw new AssertionException("Response had an empty body.");

    private static DateTimeOffset At(int day) => new(2026, 9, day, 12, 0, 0, TimeSpan.Zero);

    private static Book NewBook(string isbn, string title) =>
        new() { Isbn = isbn, Title = title, Author = "Author" };

    private static BookEntity NewCopy(string isbn, string? loanedTo = null) =>
        new() { Id = Guid.NewGuid(), BookIsbn = isbn, LoanedToMemberId = loanedTo };

    private static LoanHistory NewLoan(BookEntity copy, string memberId, DateTimeOffset borrowedAt, DateTimeOffset? returnedAt = null) =>
        new() { Id = Guid.NewGuid(), BookCopyId = copy.Id, MemberId = memberId, BorrowedAt = borrowedAt, ReturnedAt = returnedAt };

    private static BookStats NewStats(string isbn, int avgLoanTime, int borrowedCount) =>
        new() { Id = Guid.NewGuid(), BookIsbn = isbn, AvgLoanTime = avgLoanTime, BorrowedCount = borrowedCount };
}
