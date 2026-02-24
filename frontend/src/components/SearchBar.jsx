import { useEffect, useMemo, useState } from "react";
import { api } from "../api/axios";
import { useAuth } from "../context/AuthContext";

export default function SearchBar() {
  const { user } = useAuth();
  const [query, setQuery] = useState("");
  const [results, setResults] = useState(null);
  const [open, setOpen] = useState(false);

  useEffect(() => {
    if (query.trim().length < 2) {
      setResults(null);
      setOpen(false);
      return;
    }

    const handle = setTimeout(async () => {
      try {
        const data = await api.request(`/search?q=${encodeURIComponent(query.trim())}`);
        setResults(data);
        setOpen(true);
      } catch {
        setResults(null);
        setOpen(false);
      }
    }, 300);

    return () => clearTimeout(handle);
  }, [query]);

  const sections = useMemo(() => {
    if (!results) return [];

    const list = [];
    if (results.users?.length) {
      list.push({
        title: "Employees",
        items: results.users.slice(0, 5).map((u) => `${u.fullName} (${u.role})`),
      });
    }
    if (results.payments?.length) {
      list.push({
        title: "Payments",
        items: results.payments.slice(0, 5).map((p) => `$${p.amount} • ${p.status}`),
      });
    }
    if (results.leaves?.length) {
      list.push({
        title: "PTO",
        items: results.leaves.slice(0, 5).map((l) => `${l.status} • ${new Date(l.startDate).toLocaleDateString()}`),
      });
    }
    if (results.auditLogs?.length) {
      list.push({
        title: "Audit Logs",
        items: results.auditLogs.slice(0, 5).map((l) => `${l.action} • ${new Date(l.timestamp).toLocaleDateString()}`),
      });
    }
    if (results.documents?.length) {
      list.push({
        title: "Documents",
        items: results.documents.slice(0, 5).map((d) => d.fileName),
      });
    }
    return list;
  }, [results]);

  return (
    <div className="relative w-full max-w-md">
      <input
        className="w-full rounded-full border border-slate-300 bg-white px-4 py-2 text-sm text-slate-700 shadow-sm outline-none transition focus:border-slate-400 dark:border-slate-700 dark:bg-slate-950 dark:text-slate-200"
        placeholder={`Search ${user?.role === "HR" ? "employees, payments, logs" : user?.role === "Manager" ? "team & PTO" : "your records"}`}
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        onFocus={() => setOpen(!!results)}
      />

      {open && sections.length > 0 && (
        <div className="absolute left-0 right-0 z-20 mt-2 max-h-80 overflow-auto rounded-2xl border border-slate-200 bg-white p-4 text-sm shadow-lg dark:border-slate-800 dark:bg-slate-950">
          {sections.map((section) => (
            <div key={section.title} className="mb-3 last:mb-0">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">
                {section.title}
              </p>
              <div className="mt-2 space-y-1">
                {section.items.map((item, index) => (
                  <div key={`${section.title}-${index}`} className="text-slate-600 dark:text-slate-300">
                    {item}
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
