import type { Metadata } from "next";
import LoanedBooks from "@/components/LoanedBooks";

export const metadata: Metadata = {
  title: "My loans — Booker",
};

export default function LoansPage() {
  return (
    <div className="flex flex-1 flex-col items-center bg-zinc-50 dark:bg-black">
      <LoanedBooks />
    </div>
  );
}
