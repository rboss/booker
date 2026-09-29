export interface Book {
  isbn: string;
  title: string;
  author: string;
  copies: BookCopy[];
}

export interface BookStats {
  avgLoanTime: number;
  borrowedCount: number;
}

export interface BookCopy {
  id: string;
  loanedToMemberId: string | null;
}

export interface LoanHistoryEntry {
  id: string;
  bookCopyId: string;
  isbn: string;
  title: string;
  memberId: string;
  borrowedAt: string;
  returnedAt: string | null;
}
