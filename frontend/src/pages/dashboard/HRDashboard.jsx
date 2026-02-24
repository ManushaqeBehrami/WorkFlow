import { useEffect, useState } from "react";
import { api } from "../../api/axios";

export default function HRDashboard() {
  const [stats, setStats] = useState({ employees: 0, payments: 0, pendingPto: 0 });
  const [ptoQueue, setPtoQueue] = useState([]);

  useEffect(() => {
    const load = async () => {
      const [users, payments, leaves] = await Promise.all([
        api.request("/users"),
        api.request("/payments"),
        api.request("/leaves"),
      ]);

      const pending = leaves.filter((l) => l.status === "Pending");

      setStats({
        employees: users.length,
        payments: payments.length,
        pendingPto: pending.length,
      });
      setPtoQueue(pending.slice(0, 3));
    };

    load().catch(() => {});
  }, []);

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-semibold">HR Command Center</h1>
        <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">
          Monitor people operations, contracts, and payroll activity at a glance.
        </p>
      </div>

      <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {[
          { label: "Total Employees", value: stats.employees },
          { label: "Total Payments", value: stats.payments },
          { label: "Pending PTO", value: stats.pendingPto },
        ].map((stat) => (
          <div key={stat.label} className="rounded-2xl border border-slate-200/70 bg-white/80 p-5 shadow-sm dark:border-slate-800 dark:bg-slate-900/70">
            <p className="text-xs uppercase tracking-[0.2em] text-slate-400">{stat.label}</p>
            <p className="mt-3 text-2xl font-semibold text-slate-900 dark:text-white">{stat.value}</p>
          </div>
        ))}
      </section>

      <section className="grid gap-6 xl:grid-cols-[1.2fr_1fr]">
        <div className="rounded-2xl border border-slate-200/70 bg-white/80 p-6 shadow-sm dark:border-slate-800 dark:bg-slate-900/70">
          <h2 className="text-lg font-semibold">Pending PTO (view only)</h2>
          <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">
            HR can monitor PTO activity but approvals happen in manager view.
          </p>
          <div className="mt-4 space-y-3">
            {ptoQueue.map((item) => (
              <div key={item.id} className="flex items-center justify-between rounded-xl border border-slate-200/70 bg-slate-50/80 px-4 py-3 text-sm dark:border-slate-800 dark:bg-slate-950/60">
                <div>
                  <p className="font-medium text-slate-900 dark:text-white">User #{item.userId}</p>
                  <p className="text-xs text-slate-500 dark:text-slate-400">
                    {new Date(item.startDate).toLocaleDateString()} - {new Date(item.endDate).toLocaleDateString()}
                  </p>
                </div>
                <span className="rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold text-amber-700 dark:bg-amber-400/20 dark:text-amber-200">
                  {item.status}
                </span>
              </div>
            ))}
          </div>
        </div>

        <div className="rounded-2xl border border-slate-200/70 bg-white/80 p-6 shadow-sm dark:border-slate-800 dark:bg-slate-900/70">
          <h2 className="text-lg font-semibold">Today at a glance</h2>
          <ul className="mt-4 space-y-3 text-sm text-slate-600 dark:text-slate-300">
            <li>3 contracts expiring in the next 30 days.</li>
            <li>Payroll run scheduled for Friday at 9:00 AM.</li>
            <li>2 role changes waiting for HR confirmation.</li>
          </ul>
          <div className="mt-6 rounded-xl border border-dashed border-slate-300 p-4 text-sm text-slate-500 dark:border-slate-700 dark:text-slate-400">
            Use the Employees page to upload contracts, update roles, and assign managers.
          </div>
        </div>
      </section>
    </div>
  );
}
