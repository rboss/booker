import BookLibrary from "@/components/BookLibrary";

export default function Home() {

  return (
    <div className="flex flex-1 flex-col items-center bg-zinc-50 dark:bg-black">
      <BookLibrary />
    </div>
  );
}
