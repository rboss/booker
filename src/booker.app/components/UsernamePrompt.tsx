"use client";

import { useState, useSyncExternalStore } from "react";
import {
  getStoredUsername,
  setStoredUsername,
  subscribeToUsername,
} from "@/lib/username-storage";

export default function UsernamePrompt() {
  const username = useSyncExternalStore(
    subscribeToUsername,
    getStoredUsername,
    () => undefined,
  );
  const [value, setValue] = useState("");

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const trimmed = value.trim();
    if (!trimmed) return;
    setStoredUsername(trimmed);
  }

  // Not yet known on the server / first client render.
  if (username === undefined) return null;

  if (username) {
    return (
      <div className="fixed top-4 right-4 z-40 rounded-full border border-black/[.08] bg-white px-3 py-1 text-sm text-black dark:border-white/[.145] dark:bg-zinc-900 dark:text-zinc-50">
        {username}
      </div>
    );
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="w-full max-w-sm rounded-lg border border-black/[.08] bg-white p-6 dark:border-white/[.145] dark:bg-zinc-900">
        <h2 className="text-lg font-semibold text-black dark:text-zinc-50">
          Welcome to Booker
        </h2>
        <p className="mt-1 text-sm text-zinc-600 dark:text-zinc-400">
          What&apos;s your name? We&apos;ll remember it for next time.
        </p>
        <form onSubmit={handleSubmit} className="mt-4 flex flex-col gap-2">
          <input
            type="text"
            required
            autoFocus
            placeholder="Your name"
            value={value}
            onChange={(e) => setValue(e.target.value)}
            className="rounded border border-black/[.08] bg-transparent px-2 py-1 text-sm dark:border-white/[.145]"
          />
          <button
            type="submit"
            className="rounded bg-foreground px-3 py-1.5 text-sm font-medium text-background"
          >
            Continue
          </button>
        </form>
      </div>
    </div>
  );
}
