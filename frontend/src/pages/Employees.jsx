import { useCallback, useEffect, useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { api } from "../api/axios";

export default function EmployeesPage() {
  const [employees, setEmployees] = useState([]);
  const [documents, setDocuments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [searchParams] = useSearchParams();

  const loadData = useCallback(async () => {
    try {
      setLoading(true);
      const [usersData, docsData] = await Promise.all([
        api.request("/users"),
        api.request("/documents"),
      ]);
      setEmployees(usersData.filter((user) => user.role !== "HR"));
      setDocuments(docsData);
    } catch (err) {
      setError(err.message || "Unable to load employees.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const docsByUser = useMemo(() => {
    return documents.reduce((acc, doc) => {
      if (!acc[doc.userId]) acc[doc.userId] = [];
      acc[doc.userId].push(doc);
      return acc;
    }, {});
  }, [documents]);

  const handleRoleChange = async (id, role) => {
    try {
      await api.request(`/users/${id}/role?role=${role}`, "PUT");
      setEmployees((prev) => prev.map((emp) => (emp.id === id ? { ...emp, role } : emp)));
    } catch (err) {
      setError(err.message || "Unable to update role.");
    }
  };

  const handleContractUpload = async (id, file) => {
    if (!file) return;
    try {
      const formData = new FormData();
      formData.append("userId", String(id));
      formData.append("file", file);

      await api.request("/documents/upload", "POST", formData);
      await loadData();
    } catch (err) {
      setError(err.message || "Unable to upload contract metadata.");
    }
  };

  const focusedId = searchParams.get("focus");

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">Employees</h1>
        <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">
          Update roles, assign managers, and upload contracts.
        </p>
      </div>

      {loading && <p className="text-sm text-slate-400">Loading employees...</p>}
      {error && <p className="text-sm text-rose-400">{error}</p>}

      <div className="grid gap-4">
        {employees.map((emp) => {
          const userDocs = docsByUser[emp.id] || [];
          const latestDoc = userDocs[0];
          const isFocused = focusedId && String(emp.id) === String(focusedId);

          return (
            <div key={emp.id} className={`rounded-2xl border p-5 shadow-sm ${
              isFocused
                ? "border-emerald-400 bg-emerald-50/40"
                : "border-slate-200/70 bg-white/80 dark:border-slate-800 dark:bg-slate-900/70"
            }`}>
              <div className="flex flex-wrap items-center justify-between gap-4">
                <div>
                  <p className="text-lg font-semibold text-slate-900 dark:text-white">{emp.fullName}</p>
                  <p className="text-sm text-slate-500 dark:text-slate-400">{emp.email}</p>
                </div>

                <div className="flex flex-wrap items-center gap-3">
                  <select
                    value={emp.role}
                    onChange={(e) => handleRoleChange(emp.id, e.target.value)}
                    className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-700 shadow-sm dark:border-slate-700 dark:bg-slate-950 dark:text-slate-200"
                  >
                    <option value="Employee">Employee</option>
                    <option value="Manager">Manager</option>
                    <option value="HR">HR</option>
                  </select>

                  <label className="cursor-pointer rounded-lg border border-dashed border-slate-300 px-3 py-2 text-xs font-semibold text-slate-600 transition hover:border-slate-400 dark:border-slate-700 dark:text-slate-300">
                    Upload contract
                    <input
                      type="file"
                      className="hidden"
                      onChange={(e) => handleContractUpload(emp.id, e.target.files?.[0])}
                    />
                  </label>
                </div>
              </div>

              <div className="mt-4 flex flex-wrap items-center justify-between gap-3 rounded-xl border border-slate-200/70 bg-slate-50/80 px-4 py-3 text-sm dark:border-slate-800 dark:bg-slate-950/60">
                <p className="text-slate-600 dark:text-slate-300">
                  Latest contract: {latestDoc ? latestDoc.fileName : "No document"}
                </p>
                {latestDoc?.id ? (
                  <Link
                    className="rounded-full bg-slate-900 px-3 py-1 text-xs font-semibold text-white hover:bg-slate-800 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-200"
                    to={`/documents/${latestDoc.id}`}
                  >
                    View contract
                  </Link>
                ) : (
                  <span className="rounded-full border border-slate-300 px-3 py-1 text-xs font-semibold text-slate-400 dark:border-slate-700 dark:text-slate-500">
                    No file
                  </span>
                )}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
