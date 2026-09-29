import { BookActionError, checkoutBook } from "@/lib/mock-data";

export async function POST(
  request: Request,
  ctx: RouteContext<"/api/books/[id]/checkout">
) {
  const { id } = await ctx.params;
  const body = await request.json().catch(() => null);
  const borrower = typeof body?.borrower === "string" ? body.borrower.trim() : "";

  if (!borrower) {
    return Response.json({ error: "Borrower name is required" }, { status: 400 });
  }

  try {
    const book = checkoutBook(id, borrower);
    return Response.json(book);
  } catch (error) {
    if (error instanceof BookActionError) {
      return Response.json({ error: error.message }, { status: 409 });
    }
    throw error;
  }
}
