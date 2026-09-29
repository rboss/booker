"use client";

import { useEffect, useState } from "react";
import type { Book } from "@/types/book";
import { checkoutBook, listBooks, returnBook } from "@/lib/api/books";
import BookCard from "./BookCard";
import { getStoredUsername } from "@/lib/username-storage";

export default function BookLibrary() {
  const [books, setBooks] = useState<Book[]>([]);
  const [query, setQuery] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    listBooks()
      .then(setBooks)
      .catch((err) => setError(err instanceof Error ? err.message : "Could not load books"))
      .finally(() => setLoading(false));
  }, []);

  const search = query.trim().toLowerCase();
  const visibleBooks = search
    ? books.filter(
        (book) =>
          book.title.toLowerCase().includes(search) || book.author.toLowerCase().includes(search),
      )
    : books;

  async function handleCheckout(isbn: string, borrower: string) {
    const updated = await checkoutBook(isbn, borrower);
    setBooks((prev) => prev.map((book) => (book.isbn === updated.isbn ? updated : book)));
  }

  async function handleReturn(isbn: string, borrower: string) {
    const updated = await returnBook(isbn, borrower);
    setBooks((prev) => prev.map((book) => (book.isbn === updated.isbn ? updated : book)));
  }

  return (
    <div className="flex w-full max-w-5xl flex-col gap-6 px-6 py-12">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-black dark:text-zinc-50">
            Booker
          </h1>
          <p className="text-sm text-zinc-600 dark:text-zinc-400">
            Browse the shelf, search by title or author, and borrow or return a book.
          </p>
        </div>
      </div>

      <input
        type="search"
        placeholder="Search by title or author…"
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        className="w-full max-w-sm rounded border border-black/[.08] bg-transparent px-3 py-2 text-sm dark:border-white/[.145]"
      />

      {error && <p className="text-sm text-red-600 dark:text-red-400">{error}</p>}

      {visibleBooks.length === 0 && !loading ? (
        <p className="text-sm text-zinc-600 dark:text-zinc-400">No books match your search.</p>
      ) : (
        <div
          className={`grid grid-cols-1 gap-4 transition-opacity sm:grid-cols-3 lg:grid-cols-5 ${
            loading ? "opacity-60" : ""
          }`}
        >
          {visibleBooks.map((book) => (
            <BookCard
              key={book.isbn}
              book={book}
              username={getStoredUsername()}
              onCheckout={handleCheckout}
              onReturn={handleReturn}
            />
          ))}
        </div>
      )}
    </div>
  );
}
