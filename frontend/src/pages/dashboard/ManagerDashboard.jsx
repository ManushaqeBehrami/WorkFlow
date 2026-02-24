import { useEffect, useMemo, useState } from "react";
import { api } from "../../api/axios";

export default function ManagerDashboard() {
  const [requests, setRequests] = useState([]);

  useEffect(() => {
    api.request("/leaves").then(setRequests).catch(() => {});
  }, []);

  const pendingCount = useMemo(
    () => requests.filter((r) => r.status === "Pending").length,
    [requests]
  );

  const handleDecision = async (id, status) => {
    await api.request(`/leaves/${id}/status?status=${status}`, "PUT");
    const refreshed = await api.request("/leaves");
    setRequests(refreshed);
  };

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-semibold">Manager Overview</h1>
        <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">
          Review team activity and approve PTO requests assigned to you.
        </p>
      </div>

      <section className="grid gap-4 md:grid-cols-3">
        {[
          { label: "Team Members", value: "8" },
          { label: "Pending PTO", value: String(pendingCount) },
          { label: "Open Projects", value: "5" },
        ].map((stat) => (
          <div key={stat.label} className="rounded-2xl border border-slate-200/70 bg-white/80 p-5 shadow-sm dark:border-slate-800 dark:bg-slate-900/70">
            <p className="text-xs uppercase tracking-[0.2em] text-slate-400">{stat.label}</p>
            <p className="mt-3 text-2xl font-semibold text-slate-900 dark:text-white">{stat.value}</p>
          </div>
        ))}
      </section>

      <section className="rounded-2xl border border-slate-200/70 bg-white/80 p-6 shadow-sm dark:border-slate-800 dark:bg-slate-900/70">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold">PTO approvals</h2>
          <span className="rounded-full bg-slate-900 px-3 py-1 text-xs font-semibold text-white dark:bg-slate-100 dark:text-slate-900">
            {pendingCount} pending
          </span>
        </div>
        <div className="mt-4 space-y-3">
          {requests.map((req) => (
            <div key={req.id} className="flex flex-wrap items-center justify-between gap-4 rounded-xl border border-slate-200/70 bg-slate-50/80 px-4 py-3 text-sm dark:border-slate-800 dark:bg-slate-950/60">
              <div>
                <p className="font-medium text-slate-900 dark:text-white">User #{req.userId}</p>
                <p className="text-xs text-slate-500 dark:text-slate-400">{new Date(req.startDate).toLocaleDateString()} - {new Date(req.endDate).toLocaleDateString()} - {req.reason}</p>
              </div>
              <div className="flex items-center gap-2">
                <span className={`rounded-full px-3 py-1 text-xs font-semibold ${
                  req.status === "Approved"
                    ? "bg-emerald-100 text-emerald-700 dark:bg-emerald-400/20 dark:text-emerald-200"
                    : req.status === "Declined"
                    ? "bg-rose-100 text-rose-700 dark:bg-rose-400/20 dark:text-rose-200"
                    : "bg-amber-100 text-amber-700 dark:bg-amber-400/20 dark:text-amber-200"
                }`}>
                  {req.status}
                </span>
                <button
                  type="button"
                  onClick={() => handleDecision(req.id, "Approved")}
                  className="rounded-full bg-emerald-500 px-3 py-1 text-xs font-semibold text-white hover:bg-emerald-400"
                >
                  Approve
                </button>
                <button
                  type="button"
                  onClick={() => handleDecision(req.id, "Declined")}
                  className="rounded-full bg-rose-500 px-3 py-1 text-xs font-semibold text-white hover:bg-rose-400"
                >
                  Decline
                </button>
              </div>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}
