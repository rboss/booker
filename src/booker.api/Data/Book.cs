namespace Booker.Api.Data;

public record ISBN(string Value)
{
    public static ISBN Parse(string value) => new(value);
}

public sealed class Book
{
    public required string Isbn { get; set; }

    public required string Title { get; set; }

    public required string Author { get; set; }

    public ICollection<BookEntity> Copies { get; set; } = [];
}
