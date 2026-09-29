import Link from "next/link";

export default function SiteNav() {
  return (
    <nav className="flex justify-center border-b border-black/[.08] bg-white dark:border-white/[.145] dark:bg-zinc-900">
      <div className="flex w-full max-w-5xl gap-4 px-6 py-3 text-sm font-medium">
        <Link href="/" className="text-black hover:underline dark:text-zinc-50">
          Library
        </Link>
        <Link href="/loans" className="text-black hover:underline dark:text-zinc-50">
          My loans
        </Link>
      </div>
    </nav>
  );
}
