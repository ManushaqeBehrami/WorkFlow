import { useEffect, useState } from "react";
import { useAuth } from "../../context/AuthContext";
import { api } from "../../api/axios";

export default function EmployeeDashboard() {
  const { user } = useAuth();
  const [requests, setRequests] = useState([]);
  const [documents, setDocuments] = useState([]);

  useEffect(() => {
    api.request("/me/leaves").then(setRequests).catch(() => {});
    api.request("/documents/me").then(setDocuments).catch(() => {});
  }, []);

  const latestDoc = documents[0];

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-semibold">My Dashboard</h1>
        <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">
          View your profile, contract, and PTO activity.
        </p>
      </div>

      <section className="grid gap-6 md:grid-cols-[1.2fr_1fr]">
        <div className="rounded-2xl border border-slate-200/70 bg-white/80 p-6 shadow-sm dark:border-slate-800 dark:bg-slate-900/70">
          <h2 className="text-lg font-semibold">Profile</h2>
          <div className="mt-4 space-y-2 text-sm text-slate-600 dark:text-slate-300">
            <p><span className="font-medium text-slate-900 dark:text-white">Email:</span> {user?.email}</p>
            <p><span className="font-medium text-slate-900 dark:text-white">Role:</span> {user?.role}</p>
            <p><span className="font-medium text-slate-900 dark:text-white">Manager:</span> Unassigned</p>
            <p><span className="font-medium text-slate-900 dark:text-white">Department:</span> Product Design</p>
          </div>
        </div>

        <div className="rounded-2xl border border-slate-200/70 bg-white/80 p-6 shadow-sm dark:border-slate-800 dark:bg-slate-900/70">
          <h2 className="text-lg font-semibold">Contract</h2>
          <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">
            Latest contract uploaded by HR.
          </p>
          <div className="mt-4 rounded-xl border border-dashed border-slate-300 bg-slate-50/80 p-4 text-sm text-slate-600 dark:border-slate-700 dark:bg-slate-950/60 dark:text-slate-300">
            {latestDoc ? latestDoc.fileName : "No contract uploaded"}
          </div>
          <button className="mt-4 w-full rounded-lg bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-200">
            Download contract
          </button>
        </div>
      </section>

      <section className="rounded-2xl border border-slate-200/70 bg-white/80 p-6 shadow-sm dark:border-slate-800 dark:bg-slate-900/70">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold">PTO requests</h2>
          <button className="rounded-full bg-emerald-500 px-3 py-1 text-xs font-semibold text-slate-950 hover:bg-emerald-400">
            New request
          </button>
        </div>
        <div className="mt-4 space-y-3">
          {requests.map((req) => (
            <div key={req.id} className="flex items-center justify-between rounded-xl border border-slate-200/70 bg-slate-50/80 px-4 py-3 text-sm dark:border-slate-800 dark:bg-slate-950/60">
              <div>
                <p className="font-medium text-slate-900 dark:text-white">
                  {new Date(req.startDate).toLocaleDateString()} - {new Date(req.endDate).toLocaleDateString()}
                </p>
                <p className="text-xs text-slate-500 dark:text-slate-400">{req.reason}</p>
              </div>
              <span className={`rounded-full px-3 py-1 text-xs font-semibold ${
                req.status === "Approved"
                  ? "bg-emerald-100 text-emerald-700 dark:bg-emerald-400/20 dark:text-emerald-200"
                  : req.status === "Declined"
                  ? "bg-rose-100 text-rose-700 dark:bg-rose-400/20 dark:text-rose-200"
                  : "bg-amber-100 text-amber-700 dark:bg-amber-400/20 dark:text-amber-200"
              }`}>
                {req.status}
              </span>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}
