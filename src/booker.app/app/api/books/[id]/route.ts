import { getBook as getBookMock } from "@/lib/mock-data";
import { getBook } from '@/lib/api/books';

export async function GET(
  _request: Request,
  ctx: RouteContext<"/api/books/[id]">
) {
  const { id } = await ctx.params;
  const book = await getBook(id);
  if (!book) {
    return Response.json({ error: "Book not found" }, { status: 404 });
  }
  return Response.json(book);
}
