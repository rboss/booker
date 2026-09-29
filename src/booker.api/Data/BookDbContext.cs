using Microsoft.EntityFrameworkCore;

namespace Booker.Api.Data;

public sealed class BookDbContext(DbContextOptions<BookDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookEntity> BookEntities => Set<BookEntity>();
    public DbSet<LoanHistory> LoanHistory => Set<LoanHistory>();
    public DbSet<BookStats> BookStats => Set<BookStats>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookEntity>(entity =>
        {
            entity.HasKey(copy => copy.Id);
            entity.HasOne(copy => copy.Book)
                .WithMany(book => book.Copies)
                .HasForeignKey(copy => copy.BookIsbn)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LoanHistory>(entity =>
        {
            entity.HasKey(loan => loan.Id);
            entity.HasOne(loan => loan.BookCopy)
                .WithMany()
                .HasForeignKey(loan => loan.BookCopyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookStats>(entity =>
        {
            entity.HasKey(stat => stat.Id);
            entity.HasOne(stat => stat.Book)
                .WithMany()
                .HasForeignKey(stat => stat.BookIsbn)
                .HasPrincipalKey(book => book.Isbn)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(book => book.Isbn);
            entity.HasData(
                new Book { Isbn = "9780441478125", Title = "The Left Hand of Darkness", Author = "Ursula K. Le Guin" },
                new Book { Isbn = "9781472258229", Title = "Kindred", Author = "Octavia E. Butler" },
                new Book { Isbn = "9781399638302", Title = "The Dispossessed", Author = "Ursula K. Le Guin" },
                new Book { Isbn = "9780099478393", Title = "The Quiet American", Author = "Graham Greene" },
                new Book { Isbn = "9780099512158", Title = "The Leopard", Author = "Giuseppe Tomasi Di Lampedusa" },
                new Book { Isbn = "9780547928227", Title = "The Hobbit", Author = "J. R. R. Tolkien" },
                new Book { Isbn = "9780441172719", Title = "Dune", Author = "Frank Herbert" },
                new Book { Isbn = "9781451673319", Title = "Fahrenheit 451", Author = "Ray Bradbury" },
                new Book { Isbn = "9780385490818", Title = "The Handmaid's Tale", Author = "Margaret Atwood" },
                new Book { Isbn = "9780553418026", Title = "The Martian", Author = "Andy Weir" });

        });

        modelBuilder.Entity<BookStats>().HasData(
            CreateBookStats("40000000-0000-0000-0000-000000000001", "9780441478125", 18, 12),
            CreateBookStats("40000000-0000-0000-0000-000000000002", "9781472258229", 21, 17),
            CreateBookStats("40000000-0000-0000-0000-000000000003", "9781399638302", 16, 9),
            CreateBookStats("40000000-0000-0000-0000-000000000004", "9780099478393", 24, 14),
            CreateBookStats("40000000-0000-0000-0000-000000000005", "9780099512158", 19, 11),
            CreateBookStats("40000000-0000-0000-0000-000000000006", "9780547928227", 15, 20),
            CreateBookStats("40000000-0000-0000-0000-000000000007", "9780441172719", 23, 18),
            CreateBookStats("40000000-0000-0000-0000-000000000008", "9781451673319", 17, 13),
            CreateBookStats("40000000-0000-0000-0000-000000000009", "9780385490818", 20, 16),
            CreateBookStats("40000000-0000-0000-0000-000000000010", "9780553418026", 14, 22),
            CreateBookStats("40000000-0000-0000-0000-000000000011", "9780441478125", 20, 15),
            CreateBookStats("40000000-0000-0000-0000-000000000012", "9781472258229", 19, 19),
            CreateBookStats("40000000-0000-0000-0000-000000000013", "9781399638302", 17, 12),
            CreateBookStats("40000000-0000-0000-0000-000000000014", "9780099478393", 22, 16),
            CreateBookStats("40000000-0000-0000-0000-000000000015", "9780099512158", 18, 14),
            CreateBookStats("40000000-0000-0000-0000-000000000016", "9780547928227", 16, 23),
            CreateBookStats("40000000-0000-0000-0000-000000000017", "9780441172719", 21, 20),
            CreateBookStats("40000000-0000-0000-0000-000000000018", "9781451673319", 18, 15),
            CreateBookStats("40000000-0000-0000-0000-000000000019", "9780385490818", 19, 18),
            CreateBookStats("40000000-0000-0000-0000-000000000020", "9780553418026", 15, 24));

        modelBuilder.Entity<BookEntity>().HasData(
            CreateCopy("20000000-0000-0000-0000-000000000001", "9780441478125"),
            CreateCopy("20000000-0000-0000-0000-000000000002", "9780441478125"),
            CreateCopy("20000000-0000-0000-0000-000000000003", "9780441478125", "Priya"),
            CreateCopy("20000000-0000-0000-0000-000000000004", "9781472258229", "Marcus"),
            CreateCopy("20000000-0000-0000-0000-000000000005", "9781472258229", "Ada"),
            CreateCopy("20000000-0000-0000-0000-000000000006", "9781472258229", "Rick"),
            CreateCopy("20000000-0000-0000-0000-000000000007", "9781472258229", "Theo"),
            CreateCopy("20000000-0000-0000-0000-000000000008", "9781399638302", "Ada"),
            CreateCopy("20000000-0000-0000-0000-000000000009", "9781399638302"),
            CreateCopy("20000000-0000-0000-0000-000000000010", "9781399638302"),
            CreateCopy("20000000-0000-0000-0000-000000000011", "9781399638302"),
            CreateCopy("20000000-0000-0000-0000-000000000012", "9781399638302", "Sam"),
            CreateCopy("20000000-0000-0000-0000-000000000013", "9780099478393", "Lina"),
            CreateCopy("20000000-0000-0000-0000-000000000014", "9780099478393", "Noah"),
            CreateCopy("20000000-0000-0000-0000-000000000015", "9780099478393", "Riley"),
            CreateCopy("20000000-0000-0000-0000-000000000016", "9780099478393"),
            CreateCopy("20000000-0000-0000-0000-000000000017", "9780099478393"),
            CreateCopy("20000000-0000-0000-0000-000000000018", "9780099478393"),
            CreateCopy("20000000-0000-0000-0000-000000000019", "9780099512158"),
            CreateCopy("20000000-0000-0000-0000-000000000020", "9780099512158", "Theo"),
            CreateCopy("20000000-0000-0000-0000-000000000021", "9780099512158"),
            CreateCopy("20000000-0000-0000-0000-000000000022", "9780099512158"),
            CreateCopy("20000000-0000-0000-0000-000000000023", "9780099512158"),
            CreateCopy("20000000-0000-0000-0000-000000000024", "9780099512158"),
            CreateCopy("20000000-0000-0000-0000-000000000025", "9780099512158"),
            CreateCopy("20000000-0000-0000-0000-000000000026", "9780547928227"),
            CreateCopy("20000000-0000-0000-0000-000000000027", "9780547928227"),
            CreateCopy("20000000-0000-0000-0000-000000000028", "9780547928227"),
            CreateCopy("20000000-0000-0000-0000-000000000029", "9780441172719"),
            CreateCopy("20000000-0000-0000-0000-000000000030", "9780441172719"),
            CreateCopy("20000000-0000-0000-0000-000000000031", "9780441172719"),
            CreateCopy("20000000-0000-0000-0000-000000000032", "9781451673319"),
            CreateCopy("20000000-0000-0000-0000-000000000033", "9781451673319"),
            CreateCopy("20000000-0000-0000-0000-000000000034", "9781451673319"),
            CreateCopy("20000000-0000-0000-0000-000000000035", "9780385490818"),
            CreateCopy("20000000-0000-0000-0000-000000000036", "9780385490818"),
            CreateCopy("20000000-0000-0000-0000-000000000037", "9780385490818"),
            CreateCopy("20000000-0000-0000-0000-000000000038", "9780553418026"),
            CreateCopy("20000000-0000-0000-0000-000000000039", "9780553418026"),
            CreateCopy("20000000-0000-0000-0000-000000000040", "9780553418026"));

        // Open loans matching the copies seeded above as loaned, plus older returned loans.
        modelBuilder.Entity<LoanHistory>().HasData(
            CreateLoan("30000000-0000-0000-0000-000000000001", "20000000-0000-0000-0000-000000000003", "Priya", "2026-09-01T10:15:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000002", "20000000-0000-0000-0000-000000000004", "Marcus", "2026-09-03T14:30:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000003", "20000000-0000-0000-0000-000000000005", "Ada", "2026-09-05T09:00:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000004", "20000000-0000-0000-0000-000000000006", "Rick", "2026-09-08T16:45:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000005", "20000000-0000-0000-0000-000000000007", "Theo", "2026-09-10T11:20:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000006", "20000000-0000-0000-0000-000000000008", "Ada", "2026-09-12T13:05:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000007", "20000000-0000-0000-0000-000000000012", "Sam", "2026-09-14T08:40:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000008", "20000000-0000-0000-0000-000000000013", "Lina", "2026-09-15T17:10:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000009", "20000000-0000-0000-0000-000000000014", "Noah", "2026-09-17T12:00:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000010", "20000000-0000-0000-0000-000000000015", "Riley", "2026-09-19T15:25:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000011", "20000000-0000-0000-0000-000000000020", "Theo", "2026-09-21T10:50:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000012", "20000000-0000-0000-0000-000000000001", "Morgan", "2026-06-02T09:00:00Z", "2026-06-16T11:30:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000013", "20000000-0000-0000-0000-000000000002", "Jordan", "2026-06-10T13:20:00Z", "2026-06-28T15:45:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000014", "20000000-0000-0000-0000-000000000009", "Priya", "2026-07-01T10:10:00Z", "2026-07-19T16:00:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000015", "20000000-0000-0000-0000-000000000010", "Sam", "2026-07-08T08:30:00Z", "2026-07-23T12:15:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000016", "20000000-0000-0000-0000-000000000011", "Lina", "2026-07-15T14:00:00Z", "2026-08-03T09:40:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000017", "20000000-0000-0000-0000-000000000016", "Ada", "2026-07-21T11:25:00Z", "2026-08-05T10:00:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000018", "20000000-0000-0000-0000-000000000017", "Noah", "2026-08-01T09:15:00Z", "2026-08-18T13:30:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000019", "20000000-0000-0000-0000-000000000018", "Riley", "2026-08-08T12:45:00Z", "2026-08-27T14:10:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000020", "20000000-0000-0000-0000-000000000019", "Marcus", "2026-08-12T10:00:00Z", "2026-08-29T16:20:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000021", "20000000-0000-0000-0000-000000000021", "Theo", "2026-08-17T15:30:00Z", "2026-09-02T09:05:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000022", "20000000-0000-0000-0000-000000000022", "Morgan", "2026-08-22T08:50:00Z", "2026-09-07T12:35:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000023", "20000000-0000-0000-0000-000000000023", "Sam", "2026-08-30T13:10:00Z", "2026-09-13T10:25:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000024", "20000000-0000-0000-0000-000000000024", "Priya", "2026-09-04T11:40:00Z", "2026-09-18T15:00:00Z"),
            CreateLoan("30000000-0000-0000-0000-000000000025", "20000000-0000-0000-0000-000000000025", "Jordan", "2026-09-06T09:35:00Z", "2026-09-20T14:50:00Z"));
    }

    private static BookEntity CreateCopy(string id, string isbn, string? memberId = null) =>
        new() { Id = Guid.Parse(id), BookIsbn = isbn, LoanedToMemberId = memberId };

    private static BookStats CreateBookStats(string id, string isbn, int averageLoanTime, int borrowedCount) =>
        new()
        {
            Id = Guid.Parse(id),
            BookIsbn = isbn,
            AvgLoanTime = averageLoanTime,
            BorrowedCount = borrowedCount,
        };

    private static LoanHistory CreateLoan(string id, string copyId, string memberId, string borrowedAt, string? returnedAt = null) =>
        new()
        {
            Id = Guid.Parse(id),
            BookCopyId = Guid.Parse(copyId),
            MemberId = memberId,
            BorrowedAt = DateTimeOffset.Parse(borrowedAt),
            ReturnedAt = returnedAt is null ? null : DateTimeOffset.Parse(returnedAt),
        };
}