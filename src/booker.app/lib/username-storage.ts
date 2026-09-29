const USERNAME_KEY = "booker_username";
const CHANGE_EVENT = "booker_username_change";

export function getStoredUsername(): string | null {
  if (typeof window === "undefined") return null;
  try {
    return window.localStorage.getItem(USERNAME_KEY);
  } catch {
    return null;
  }
}

export function setStoredUsername(username: string): void {
  try {
    window.localStorage.setItem(USERNAME_KEY, username);
  } catch {
    // Storage unavailable (e.g. private mode); username won't persist.
  }
  window.dispatchEvent(new Event(CHANGE_EVENT));
}

export function subscribeToUsername(callback: () => void): () => void {
  window.addEventListener(CHANGE_EVENT, callback);
  window.addEventListener("storage", callback);
  return () => {
    window.removeEventListener(CHANGE_EVENT, callback);
    window.removeEventListener("storage", callback);
  };
}
