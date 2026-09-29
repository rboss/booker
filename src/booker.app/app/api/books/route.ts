import type { NextRequest } from "next/server";
import { listBooks } from "@/lib/api/books";

export async function GET(request: NextRequest) {
  const query = request.nextUrl.searchParams.get("q") ?? undefined;
  return Response.json(await listBooks(query));
}
