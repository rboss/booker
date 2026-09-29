// Legacy mock shape, independent of the API's Book type.
interface Book {
  id: string;
  title: string;
  author: string;
  isbn: string;
  genre?: string;
  publishedYear: number;
  coverColor: string;
  status: "available" | "borrowed";
  borrower: string | null;
}

// In-memory mock "database". Resets whenever the server process restarts.
const books: Book[] = [
  {
    id: "1",
    title: "The Left Hand of Darkness",
    author: "Ursula K. Le Guin",
    isbn: "978-0441478125",
    genre: "Science Fiction",
    publishedYear: 1969,
    coverColor: "#4a6fa5",
    status: "available",
    borrower: null,
  },
  {
    id: "2",
    title: "Kindred",
    author: "Octavia E. Butler",
    isbn: "978-0807083697",
    genre: "Science Fiction",
    publishedYear: 1979,
    coverColor: "#a54a4a",
    status: "borrowed",
    borrower: "Priya N.",
  },
  {
    id: "3",
    title: "Piranesi",
    author: "Susanna Clarke",
    isbn: "978-1635575637",
    genre: "Fantasy",
    publishedYear: 2020,
    coverColor: "#6f4a94",
    status: "available",
    borrower: null,
  },
  {
    id: "4",
    title: "Sapiens: A Brief History of Humankind",
    author: "Yuval Noah Harari",
    isbn: "978-0062316097",
    genre: "Nonfiction",
    publishedYear: 2011,
    coverColor: "#94824a",
    status: "borrowed",
    borrower: "Marcus T.",
  },
  {
    id: "5",
    title: "The Fifth Season",
    author: "N. K. Jemisin",
    isbn: "978-0316229296",
    genre: "Fantasy",
    publishedYear: 2015,
    coverColor: "#4a9478",
    status: "available",
    borrower: null,
  },
  {
    id: "6",
    title: "Educated",
    author: "Tara Westover",
    isbn: "978-0399590504",
    genre: "Memoir",
    publishedYear: 2018,
    coverColor: "#4a7d94",
    status: "available",
    borrower: null,
  },
  {
    id: "7",
    title: "Project Hail Mary",
    author: "Andy Weir",
    isbn: "978-0593135204",
    genre: "Science Fiction",
    publishedYear: 2021,
    coverColor: "#94664a",
    status: "borrowed",
    borrower: "Priya N.",
  },
  {
    id: "8",
    title: "Circe",
    author: "Madeline Miller",
    isbn: "978-0316556347",
    genre: "Fantasy",
    publishedYear: 2018,
    coverColor: "#7d4a94",
    status: "available",
    borrower: null,
  },
  {
    id: "9",
    title: "The Warmth of Other Suns",
    author: "Isabel Wilkerson",
    isbn: "978-0679763888",
    genre: "History",
    publishedYear: 2010,
    coverColor: "#4a5a94",
    status: "available",
    borrower: null,
  },
  {
    id: "10",
    title: "Klara and the Sun",
    author: "Kazuo Ishiguro",
    isbn: "978-0571364879",
    genre: "Science Fiction",
    publishedYear: 2021,
    coverColor: "#944a6f",
    status: "available",
    borrower: null,
  },
];

function matches(book: Book, query: string): boolean {
  const q = query.trim().toLowerCase();
  if (!q) return true;
  return (
    book.title.toLowerCase().includes(q) ||
    book.author.toLowerCase().includes(q)
  );
}

export function listBooks(query?: string): Book[] {
  return query ? books.filter((book) => matches(book, query)) : books;
}

export function getBook(id: string): Book | undefined {
  return books.find((book) => book.id === id);
}

export class BookActionError extends Error {}

export function checkoutBook(id: string, borrower: string): Book {
  const book = getBook(id);
  if (!book) throw new BookActionError("Book not found");
  if (book.status === "borrowed") {
    throw new BookActionError("Book is already borrowed");
  }
  book.status = "borrowed";
  book.borrower = borrower;
  return book;
}

export function returnBook(id: string): Book {
  const book = getBook(id);
  if (!book) throw new BookActionError("Book not found");
  if (book.status === "available") {
    throw new BookActionError("Book is not currently borrowed");
  }
  book.status = "available";
  book.borrower = null;
  return book;
}
