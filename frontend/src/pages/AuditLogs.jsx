import { useEffect, useState } from "react";
import { api } from "../api/axios";

export default function AuditLogsPage() {
  const [logs, setLogs] = useState([]);
  const [error, setError] = useState("");

  useEffect(() => {
    api
      .request("/audit-logs")
      .then(setLogs)
      .catch((err) => setError(err.message || "Unable to load audit logs."));
  }, []);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">Audit Logs</h1>
        <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">
          Track every sensitive change across the platform.
        </p>
      </div>

      {error && <p className="text-sm text-rose-400">{error}</p>}

      <div className="space-y-3">
        {logs.map((log) => (
          <div key={log.id} className="rounded-2xl border border-slate-200/70 bg-white/80 p-4 text-sm shadow-sm dark:border-slate-800 dark:bg-slate-900/70">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <p className="font-semibold text-slate-900 dark:text-white">{log.action}</p>
              <span className="text-xs text-slate-500 dark:text-slate-400">
                {new Date(log.timestamp).toLocaleString()}
              </span>
            </div>
            <p className="mt-2 text-slate-600 dark:text-slate-300">{log.details}</p>
          </div>
        ))}
      </div>
    </div>
  );
}
