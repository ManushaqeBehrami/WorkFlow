import { useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../../api/axios";

export default function ForgotPassword() {
  const [email, setEmail] = useState("");
  const [status, setStatus] = useState("idle");
  const [error, setError] = useState("");

  const submit = async (e) => {
    e.preventDefault();
    setStatus("loading");
    setError("");

    try {
      await api.request("/auth/forgot-password", "POST", { email });
      setStatus("sent");
    } catch (err) {
      setError(err.message || "Unable to send reset email.");
      setStatus("idle");
    }
  };

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100">
      <div className="mx-auto flex min-h-screen max-w-xl items-center px-6">
        <form onSubmit={submit} className="w-full space-y-5 rounded-2xl border border-slate-800 bg-slate-900/80 p-8">
          <div>
            <p className="text-xs uppercase tracking-[0.25em] text-slate-400">WorkFlow</p>
            <h2 className="mt-2 text-2xl font-semibold text-white">Reset your password</h2>
            <p className="text-sm text-slate-400">
              Enter the email associated with your account and we will send a reset link.
            </p>
          </div>

          <label className="block text-sm font-medium text-slate-200">
            Email
            <input
              className="mt-2 w-full rounded-lg border border-slate-700 bg-slate-950 px-4 py-2 text-sm text-white outline-none transition focus:border-slate-500"
              placeholder="name@company.com"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              disabled={status === "sent"}
            />
          </label>

          {error && <p className="text-sm text-rose-400">{error}</p>}
          {status === "sent" && (
            <p className="text-sm text-emerald-300">
              If that email exists, a reset link has been sent.
            </p>
          )}

          <button
            className="w-full rounded-lg bg-emerald-500 px-4 py-2 text-sm font-semibold text-slate-950 transition hover:bg-emerald-400 disabled:cursor-not-allowed disabled:opacity-60"
            disabled={status === "loading" || status === "sent"}
          >
            {status === "loading" ? "Sending..." : "Send reset link"}
          </button>

          <p className="text-sm text-slate-400">
            Remembered it?{" "}
            <Link to="/login" className="text-emerald-400 hover:text-emerald-300">
              Back to login
            </Link>
          </p>
        </form>
      </div>
    </div>
  );
}
