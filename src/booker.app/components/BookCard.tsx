"use client";

import Image from "next/image";
import Link from "next/link";
import { useState } from "react";
import type { Book } from "@/types/book";

interface BookCardProps {
  book: Book;
  username: string | null;
  onCheckout: (id: string, borrower: string) => Promise<void>;
  onReturn: (id: string, borrower: string) => Promise<void>;
}

export default function BookCard({ book, username, onCheckout, onReturn }: BookCardProps) {
  const [pending, setPending] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const availableCopies = book.copies.filter((copy) => copy.loanedToMemberId === null).length;

  const bookStatus =
    book.copies.some((copy) => copy.loanedToMemberId === username) ? "borrowed" :
      book.copies.some((copy) => copy.loanedToMemberId === null) ? "available" : "not available";

  async function borrowBook(bookIsbn: string) {
    setPending(true);
    setError(null);
    try {
      await onCheckout(String(bookIsbn), username ?? "");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong");
    } finally {
      setPending(false);
    }
  }

  async function handleReturn() {
    setPending(true);
    setError(null);
    try {
      await onReturn(book.isbn, username ?? "");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong");
    } finally {
      setPending(false);
    }
  }

  return (
    <div className="flex flex-col overflow-hidden rounded-lg border border-black/[.08] bg-white dark:border-white/[.145] dark:bg-zinc-900">
      <Link
        href={`/books/${book.isbn}`}
        className="relative flex h-48 items-center justify-center text-3xl font-semibold text-white"
      >
        <Image
          src={`https://covers.openlibrary.org/b/isbn/${book.isbn}-M.jpg`}
          alt={book.title}
          fill
          sizes="(min-width: 1024px) 320px, (min-width: 640px) 50vw, 100vw"
          className="object-cover"
          loading="lazy"
        />
      </Link>
      <div className="flex flex-1 flex-col gap-2 p-4">
        <div>
          <h3 className="font-semibold leading-snug text-black dark:text-zinc-50">
            <Link href={`/books/${book.isbn}`} className="hover:underline">
              {book.title}
            </Link>
          </h3>
        </div>

        <span
          className={`w-fit rounded-full px-2 py-0.5 text-xs font-medium ${bookStatus === "borrowed"
            ? "bg-sky-100 text-sky-800 dark:bg-sky-900/40 dark:text-sky-300"
            : bookStatus === "available"
              ? "bg-emerald-100 text-emerald-800 dark:bg-emerald-900/40 dark:text-emerald-300"
              : "bg-amber-100 text-amber-800 dark:bg-amber-900/40 dark:text-amber-300"
            }`}
        >
          {bookStatus === "borrowed" ? "Borrowed" : bookStatus === "available" ? "Available" : "Not available"}
        </span>

        <p className="text-xs text-zinc-600 dark:text-zinc-400">
          Available: {availableCopies}/{book.copies.length}
        </p>

        {error && <p className="text-xs text-red-600 dark:text-red-400">{error}</p>}

        <div className="mt-auto pt-2">
          {bookStatus === "borrowed" ? (
            <button
              onClick={handleReturn}
              disabled={pending}
              className="w-full rounded border border-black/[.08] px-3 py-1.5 text-sm font-medium disabled:opacity-50 dark:border-white/[.145]"
            >
              {pending ? "Returning…" : "Return"}
            </button>
          ) : bookStatus === "available" ? (
            (
              <button
                className="w-full rounded bg-foreground px-3 py-1.5 text-sm font-medium text-background"
                onClick={() => borrowBook(book.isbn)}
              >
                Borrow
              </button>
            )
          ) : (
            <button
              disabled
              className="w-full cursor-not-allowed rounded bg-zinc-300 px-3 py-1.5 text-sm font-medium text-zinc-500 opacity-60 dark:bg-zinc-700 dark:text-zinc-400"
            >
              Borrow
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
