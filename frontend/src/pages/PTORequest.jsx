import { useEffect, useMemo, useState } from "react";
import { useAuth } from "../context/AuthContext";
import { api } from "../api/axios";

export default function PTORequest() {
  const { user } = useAuth();
  const [requests, setRequests] = useState([]);
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");
  const [reason, setReason] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  const isHR = user?.role === "HR";
  const isManager = user?.role === "Manager";

  const loadRequests = async () => {
    try {
      setLoading(true);
      if (isHR || isManager) {
        const data = await api.request("/leaves");
        setRequests(data);
      } else {
        const data = await api.request("/me/leaves");
        setRequests(data);
      }
    } catch (err) {
      setError(err.message || "Unable to load PTO requests.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadRequests();
  }, [isHR, isManager]);

  const visibleRequests = useMemo(() => requests, [requests]);

  const handleDecision = async (id, status) => {
    try {
      await api.request(`/leaves/${id}/status?status=${status}`, "PUT");
      await loadRequests();
    } catch (err) {
      setError(err.message || "Unable to update PTO.");
    }
  };

  const submitRequest = async (e) => {
    e.preventDefault();
    if (!startDate || !endDate || !reason) return;
    if (new Date(startDate) > new Date(endDate)) {
      setError("Start date cannot be after end date.");
      return;
    }

    try {
      setError("");
      await api.request("/leaves", "POST", {
        startDate,
        endDate,
        reason,
      });
      setStartDate("");
      setEndDate("");
      setReason("");
      await loadRequests();
    } catch (err) {
      setError(err.message || "Unable to submit PTO request.");
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">PTO Requests</h1>
        <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">
          {isHR && "HR can monitor PTO, but approvals are handled by managers."}
          {isManager && "Approve or decline requests from your direct reports."}
          {!isHR && !isManager && "Submit and track your own PTO requests."}
        </p>
      </div>

      {!isHR && !isManager && (
        <form onSubmit={submitRequest} className="grid gap-4 rounded-2xl border border-slate-200/70 bg-white/80 p-5 shadow-sm dark:border-slate-800 dark:bg-slate-900/70 md:grid-cols-[1fr_1fr_1.2fr_auto]">
          <input
            type="date"
            className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-700 shadow-sm dark:border-slate-700 dark:bg-slate-950 dark:text-slate-200"
            value={startDate}
            onChange={(e) => setStartDate(e.target.value)}
          />
          <input
            type="date"
            className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-700 shadow-sm dark:border-slate-700 dark:bg-slate-950 dark:text-slate-200"
            value={endDate}
            onChange={(e) => setEndDate(e.target.value)}
          />
          <input
            className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-700 shadow-sm dark:border-slate-700 dark:bg-slate-950 dark:text-slate-200"
            placeholder="Reason"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
          />
          <button className="rounded-lg bg-emerald-500 px-4 py-2 text-sm font-semibold text-slate-950 hover:bg-emerald-400">
            Submit request
          </button>
        </form>
      )}

      {loading && <p className="text-sm text-slate-400">Loading PTO requests...</p>}
      {error && <p className="text-sm text-rose-400">{error}</p>}

      <div className="space-y-3">
        {visibleRequests.map((req) => (
          <div key={req.id} className="flex flex-wrap items-center justify-between gap-4 rounded-2xl border border-slate-200/70 bg-white/80 px-4 py-3 text-sm shadow-sm dark:border-slate-800 dark:bg-slate-900/70">
            <div>
              <p className="font-medium text-slate-900 dark:text-white">{req.userFullName || `User #${req.userId}`}</p>
              <p className="text-xs text-slate-500 dark:text-slate-400">
                {new Date(req.startDate).toLocaleDateString()} - {new Date(req.endDate).toLocaleDateString()} - {req.reason}
              </p>
            </div>
            <div className="flex flex-wrap items-center gap-2">
              <span className={`rounded-full px-3 py-1 text-xs font-semibold ${
                req.status === "Approved"
                  ? "bg-emerald-100 text-emerald-700 dark:bg-emerald-400/20 dark:text-emerald-200"
                  : req.status === "Declined"
                  ? "bg-rose-100 text-rose-700 dark:bg-rose-400/20 dark:text-rose-200"
                  : "bg-amber-100 text-amber-700 dark:bg-amber-400/20 dark:text-amber-200"
              }`}>
                {req.status}
              </span>

              {isManager && (
                <>
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
                </>
              )}
            </div>
          </div>
        ))}

        {!visibleRequests.length && !loading && (
          <div className="rounded-2xl border border-dashed border-slate-300 bg-white/70 p-6 text-sm text-slate-500 dark:border-slate-700 dark:bg-slate-900/40 dark:text-slate-400">
            No PTO requests to show.
          </div>
        )}
      </div>
    </div>
  );
}
