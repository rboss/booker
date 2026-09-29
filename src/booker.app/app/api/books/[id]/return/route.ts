import { BookActionError, returnBook } from "@/lib/mock-data";

export async function POST(
  _request: Request,
  ctx: RouteContext<"/api/books/[id]/return">
) {
  const { id } = await ctx.params;

  try {
    const book = returnBook(id);
    return Response.json(book);
  } catch (error) {
    if (error instanceof BookActionError) {
      return Response.json({ error: error.message }, { status: 409 });
    }
    throw error;
  }
}
