import type { Book, BookStats, LoanHistoryEntry } from "@/types/book";

const API_BASE_URL = "http://localhost:5152";

export class ApiError extends Error {
  status: number;

  constructor(message: string, status: number) {
    super(message);
    this.status = status;
  }
}

async function unwrap(res: Response) {
  if (!res.ok) {
    const body = await res.json().catch(() => null);
    throw new ApiError(body?.error ?? `Request failed with status ${res.status}`, res.status);
  }
  return res.json();
}

export async function listBooks(query?: string): Promise<Book[]> {
  const url = query
    ? `${API_BASE_URL}/books?q=${encodeURIComponent(query)}`
    : `${API_BASE_URL}/books`;
  return fetch(url, { cache: "no-store" }).then(unwrap);
}

export async function getBook(id: string): Promise<Book> {
  return fetch(`${API_BASE_URL}/books/${id}`, { cache: "no-store" }).then(unwrap);
}

export async function getBookStats(isbn: string): Promise<BookStats> {
  return fetch(`${API_BASE_URL}/books/${isbn}/stats`, { cache: "no-store" }).then(unwrap);
}

export async function checkoutBook(isbn: string, borrower: string): Promise<Book> {
  return fetch(`${API_BASE_URL}/books/copies/${isbn}/loan`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ memberId: borrower }),
  }).then(unwrap);
}

export async function returnBook(isbn: string, borrower: string): Promise<Book> {
  return fetch(`${API_BASE_URL}/books/copies/${isbn}/return`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ memberId: borrower }),
  }).then(unwrap);
}

export async function getMemberLoanHistory(memberId: string): Promise<LoanHistoryEntry[]> {
  return fetch(`${API_BASE_URL}/members/${encodeURIComponent(memberId)}/history`, { cache: "no-store" }).then(unwrap);
}
