import { useAuth } from "../context/AuthContext";
import { useTheme } from "../context/ThemeContext";
import SearchBar from "./SearchBar";

export default function Navbar() {
  const { logout, user } = useAuth();
  const { theme, toggleTheme } = useTheme();

  return (
    <header className="flex flex-wrap items-center justify-between gap-4 border-b border-slate-200/60 bg-white/80 px-6 py-4 backdrop-blur dark:border-slate-800 dark:bg-slate-950/80">
      <div className="min-w-[200px]">
        <p className="text-sm text-slate-500 dark:text-slate-400">Signed in as</p>
        <div className="text-lg font-semibold text-slate-900 dark:text-white">
          {user?.email}
        </div>
      </div>

      <SearchBar />

      <div className="flex flex-wrap items-center gap-3">
        <span className="rounded-full bg-slate-900 px-3 py-1 text-xs font-semibold uppercase tracking-wide text-white dark:bg-slate-100 dark:text-slate-900">
          {user?.role}
        </span>
        <button
          type="button"
          onClick={toggleTheme}
          className="rounded-full border border-slate-300 px-3 py-1 text-xs font-medium text-slate-700 transition hover:border-slate-400 hover:text-slate-900 dark:border-slate-700 dark:text-slate-200 dark:hover:border-slate-500"
        >
          {theme === "dark" ? "Light mode" : "Dark mode"}
        </button>
        <button
          onClick={logout}
          className="rounded-full bg-rose-600 px-4 py-1 text-sm font-semibold text-white transition hover:bg-rose-500"
        >
          Logout
        </button>
      </div>
    </header>
  );
}
