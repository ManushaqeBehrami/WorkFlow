import { useEffect, useMemo, useRef, useState } from "react";
import { useAuth } from "../context/AuthContext";
import { useTheme } from "../context/ThemeContext";
import { api } from "../api/axios";
import SearchBar from "./SearchBar";

export default function Navbar() {
  const { logout, user } = useAuth();
  const { theme, toggleTheme } = useTheme();
  const [notifications, setNotifications] = useState([]);
  const [open, setOpen] = useState(false);
  const [loadingNotifications, setLoadingNotifications] = useState(false);
  const menuRef = useRef(null);

  const unreadCount = useMemo(
    () => notifications.filter((n) => !n.isRead).length,
    [notifications]
  );

  const loadNotifications = async () => {
    try {
      setLoadingNotifications(true);
      const data = await api.request("/notifications/me");
      setNotifications(data || []);
    } catch {
      setNotifications([]);
    } finally {
      setLoadingNotifications(false);
    }
  };

  useEffect(() => {
    loadNotifications();
    const interval = setInterval(loadNotifications, 30000);
    return () => clearInterval(interval);
  }, []);

  useEffect(() => {
    const onClickOutside = (event) => {
      if (!menuRef.current) return;
      if (!menuRef.current.contains(event.target)) {
        setOpen(false);
      }
    };

    document.addEventListener("mousedown", onClickOutside);
    return () => document.removeEventListener("mousedown", onClickOutside);
  }, []);

  const markRead = async (id) => {
    try {
      await api.request(`/notifications/${id}/read`, "PUT");
      setNotifications((prev) => prev.map((n) => (n.id === id ? { ...n, isRead: true } : n)));
    } catch {
      // no-op
    }
  };

  const markAllRead = async () => {
    try {
      await api.request("/notifications/read-all", "PUT");
      setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
    } catch {
      // no-op
    }
  };

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

        <div className="relative" ref={menuRef}>
          <button
            type="button"
            onClick={() => setOpen((v) => !v)}
            className="relative rounded-full border border-slate-300 px-3 py-1 text-sm font-medium text-slate-700 transition hover:border-slate-400 hover:text-slate-900 dark:border-slate-700 dark:text-slate-200 dark:hover:border-slate-500"
            aria-label="Notifications"
            title="Notifications"
          >
            <svg viewBox="0 0 24 24" className="h-4 w-4" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M15 17h5l-1.4-1.4a2 2 0 0 1-.6-1.4V11a6 6 0 1 0-12 0v3.2a2 2 0 0 1-.6 1.4L4 17h5" />
              <path d="M9 17a3 3 0 0 0 6 0" />
            </svg>
            {unreadCount > 0 && (
              <span className="absolute -right-1 -top-1 inline-flex min-w-5 justify-center rounded-full bg-rose-500 px-1 text-[10px] font-semibold text-white">
                {unreadCount > 99 ? "99+" : unreadCount}
              </span>
            )}
          </button>

          {open && (
            <div className="absolute right-0 z-50 mt-2 w-96 rounded-xl border border-slate-200 bg-white p-3 shadow-xl dark:border-slate-800 dark:bg-slate-950">
              <div className="mb-2 flex items-center justify-between">
                <p className="text-sm font-semibold text-slate-900 dark:text-white">Notifications</p>
                <button
                  type="button"
                  onClick={markAllRead}
                  className="text-xs font-medium text-emerald-600 hover:text-emerald-500"
                >
                  Mark all read
                </button>
              </div>

              <div className="max-h-80 space-y-2 overflow-y-auto pr-1">
                {loadingNotifications && (
                  <p className="py-4 text-center text-xs text-slate-400">Loading notifications...</p>
                )}

                {!loadingNotifications && notifications.length === 0 && (
                  <p className="py-4 text-center text-xs text-slate-400">No notifications.</p>
                )}

                {!loadingNotifications && notifications.map((item) => (
                  <button
                    key={item.id}
                    type="button"
                    onClick={() => markRead(item.id)}
                    className={`w-full rounded-lg border px-3 py-2 text-left transition ${
                      item.isRead
                        ? "border-slate-200 bg-slate-50 text-slate-500 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-400"
                        : "border-emerald-300 bg-emerald-50 text-slate-700 dark:border-emerald-500/40 dark:bg-emerald-500/10 dark:text-slate-200"
                    }`}
                  >
                    <p className="text-xs font-medium">{item.message}</p>
                    <p className="mt-1 text-[11px] opacity-80">{new Date(item.createdAt).toLocaleString()}</p>
                  </button>
                ))}
              </div>
            </div>
          )}
        </div>

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
