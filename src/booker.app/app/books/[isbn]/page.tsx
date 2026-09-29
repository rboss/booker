import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";
import { ApiError, getBook, getBookStats } from "@/lib/api/books";

async function loadBook(isbn: string) {
  try {
    return await Promise.all([getBook(isbn), getBookStats(isbn)]);
  } catch (err) {
    if (err instanceof ApiError && err.status === 404) notFound();
    throw err;
  }
}

export default async function BookPage(props: PageProps<"/books/[isbn]">) {
  const { isbn } = await props.params;
  const [book, stats] = await loadBook(isbn);
  const availableCopies = book.copies.filter((copy) => copy.loanedToMemberId === null).length;

  return (
    <div className="flex flex-1 flex-col items-center bg-zinc-50 dark:bg-black">
      <div className="flex w-full max-w-5xl flex-col gap-6 px-6 py-12">
        <Link href="/" className="w-fit text-sm text-zinc-600 hover:underline dark:text-zinc-400">
          ← Back to library
        </Link>

        <div className="flex flex-col gap-6 sm:flex-row">
          <div className="relative h-72 w-48 shrink-0 overflow-hidden rounded-lg bg-zinc-100 dark:bg-zinc-800">
            <Image
              src={`https://covers.openlibrary.org/b/isbn/${book.isbn}-L.jpg`}
              alt={book.title}
              fill
              sizes="192px"
              className="object-cover"
              preload
            />
          </div>

          <div className="flex flex-col gap-4">
            <div>
              <h1 className="text-2xl font-semibold text-black dark:text-zinc-50">{book.title}</h1>
              <p className="text-zinc-600 dark:text-zinc-400">{book.author}</p>
              <p className="mt-1 text-xs text-zinc-500">ISBN {book.isbn}</p>
            </div>

            <dl className="grid grid-cols-1 gap-3 sm:grid-cols-3">
              <Stat
                label="Average reading time"
                value={stats.borrowedCount > 0 ? `${stats.avgLoanTime} days` : "—"}
              />
              <Stat label="Times borrowed" value={String(stats.borrowedCount)} />
              <Stat label="Available copies" value={`${availableCopies}/${book.copies.length}`} />
            </dl>
          </div>
        </div>
      </div>
    </div>
  );
}

function Stat({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border border-black/[.08] bg-white p-4 dark:border-white/[.145] dark:bg-zinc-900">
      <dt className="text-xs text-zinc-600 dark:text-zinc-400">{label}</dt>
      <dd className="mt-1 text-xl font-semibold text-black dark:text-zinc-50">{value}</dd>
    </div>
  );
}
