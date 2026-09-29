"use client";

import Image from "next/image";
import { useEffect, useState, useSyncExternalStore } from "react";
import type { Book, LoanHistoryEntry } from "@/types/book";
import { getMemberLoanHistory, listBooks, returnBook } from "@/lib/api/books";
import { getStoredUsername, subscribeToUsername } from "@/lib/username-storage";

export default function LoanedBooks() {
  const username = useSyncExternalStore(
    subscribeToUsername,
    getStoredUsername,
    () => undefined,
  );
  const [books, setBooks] = useState<Book[]>([]);
  const [history, setHistory] = useState<LoanHistoryEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!username) return;
    Promise.all([listBooks(), getMemberLoanHistory(username)])
      .then(([all, loans]) => {
        setError(null);
        setBooks(all.filter((book) => book.copies.some((copy) => copy.loanedToMemberId === username)));
        setHistory(loans);
      })
      .catch((err) => setError(err instanceof Error ? err.message : "Could not load your loans"))
      .finally(() => setLoading(false));
  }, [username]);

  async function handleReturn(isbn: string) {
    await returnBook(isbn, username ?? "");
    setBooks((prev) => prev.filter((book) => book.isbn !== isbn));
    getMemberLoanHistory(username ?? "")
      .then(setHistory)
      .catch(() => {
        // The return succeeded; history just stays stale until the next load.
      });
  }

  return (
    <div className="flex w-full max-w-5xl flex-col gap-6 px-6 py-12">
      <div>
        <h1 className="text-2xl font-semibold text-black dark:text-zinc-50">My loans</h1>
        <p className="text-sm text-zinc-600 dark:text-zinc-400">
          Books you currently have borrowed. Return them when you&apos;re done.
        </p>
      </div>

      {error && <p className="text-sm text-red-600 dark:text-red-400">{error}</p>}

      {!username || loading ? (
        <p className="text-sm text-zinc-600 dark:text-zinc-400">Loading…</p>
      ) : books.length === 0 ? (
        !error && <p className="text-sm text-zinc-600 dark:text-zinc-400">You have no books on loan.</p>
      ) : (
        <ul className="flex flex-col gap-3">
          {books.map((book) => (
            <LoanedBookRow key={book.isbn} book={book} onReturn={handleReturn} />
          ))}
        </ul>
      )}

      {username && !loading && history.length > 0 && <LoanHistoryTable history={history} />}
    </div>
  );
}

const dateFormat = new Intl.DateTimeFormat(undefined, { dateStyle: "medium", timeStyle: "short" });

function LoanHistoryTable({ history }: { history: LoanHistoryEntry[] }) {
  return (
    <section className="flex flex-col gap-3">
      <h2 className="text-lg font-semibold text-black dark:text-zinc-50">Loan history</h2>
      <div className="overflow-x-auto rounded-lg border border-black/[.08] bg-white dark:border-white/[.145] dark:bg-zinc-900">
        <table className="w-full text-left text-sm">
          <thead className="border-b border-black/[.08] text-xs uppercase text-zinc-600 dark:border-white/[.145] dark:text-zinc-400">
            <tr>
              <th className="px-3 py-2 font-medium">Book</th>
              <th className="px-3 py-2 font-medium">Borrowed</th>
              <th className="px-3 py-2 font-medium">Returned</th>
            </tr>
          </thead>
          <tbody>
            {history.map((loan) => (
              <tr key={loan.id} className="border-b border-black/[.08] last:border-0 dark:border-white/[.145]">
                <td className="px-3 py-2 text-black dark:text-zinc-50">{loan.title}</td>
                <td className="whitespace-nowrap px-3 py-2 text-zinc-600 dark:text-zinc-400">
                  {dateFormat.format(new Date(loan.borrowedAt))}
                </td>
                <td className="whitespace-nowrap px-3 py-2 text-zinc-600 dark:text-zinc-400">
                  {loan.returnedAt ? (
                    dateFormat.format(new Date(loan.returnedAt))
                  ) : (
                    <span className="rounded-full bg-sky-100 px-2 py-0.5 text-xs font-medium text-sky-800 dark:bg-sky-900/40 dark:text-sky-300">
                      On loan
                    </span>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
}

interface LoanedBookRowProps {
  book: Book;
  onReturn: (isbn: string) => Promise<void>;
}

function LoanedBookRow({ book, onReturn }: LoanedBookRowProps) {
  const [pending, setPending] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleReturn() {
    setPending(true);
    setError(null);
    try {
      await onReturn(book.isbn);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong");
      setPending(false);
    }
  }

  return (
    <li className="flex items-center gap-4 rounded-lg border border-black/[.08] bg-white p-3 dark:border-white/[.145] dark:bg-zinc-900">
      <div className="relative h-20 w-14 shrink-0 overflow-hidden rounded bg-zinc-100 dark:bg-zinc-800">
        <Image
          src={`https://covers.openlibrary.org/b/isbn/${book.isbn}-S.jpg`}
          alt={book.title}
          fill
          sizes="56px"
          className="object-cover"
        />
      </div>
      <div className="flex min-w-0 flex-1 flex-col gap-1">
        <h3 className="truncate font-semibold leading-snug text-black dark:text-zinc-50">
          {book.title}
        </h3>
        <p className="truncate text-sm text-zinc-600 dark:text-zinc-400">{book.author}</p>
        {error && <p className="text-xs text-red-600 dark:text-red-400">{error}</p>}
      </div>
      <button
        onClick={handleReturn}
        disabled={pending}
        className="shrink-0 rounded border border-black/[.08] px-3 py-1.5 text-sm font-medium disabled:opacity-50 dark:border-white/[.145]"
      >
        {pending ? "Returning…" : "Return"}
      </button>
    </li>
  );
}
