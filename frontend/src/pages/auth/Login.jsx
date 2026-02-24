import { useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../../api/axios";
import { useAuth } from "../../context/AuthContext";

export default function Login() {
  const { login } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const submit = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const data = await api.request("/auth/login", "POST", { email, password });
      login(data);
    } catch (err) {
      setError(err.message || "Login failed");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100">
      <div className="mx-auto flex min-h-screen max-w-5xl items-center px-6">
        <div className="grid w-full gap-10 md:grid-cols-[1.1fr_0.9fr]">
          <div className="space-y-6">
            <p className="text-xs uppercase tracking-[0.25em] text-slate-400">WorkFlow</p>
            <h1 className="text-4xl font-semibold leading-tight text-white">
              People operations built for distributed teams.
            </h1>
            <p className="text-base text-slate-300">
              Sign in to manage employees, contracts, PTO requests, and payroll in one secure workspace.
            </p>
            <div className="rounded-2xl border border-slate-800 bg-slate-900/60 p-6 text-sm text-slate-300">
              <p className="font-semibold text-white">Role-aware access</p>
              <p className="mt-2">
                HR sees everything, Managers approve PTO for their teams, Employees manage their profile and requests.
              </p>
            </div>
          </div>

          <form onSubmit={submit} className="space-y-5 rounded-2xl border border-slate-800 bg-slate-900/80 p-8">
            <div>
              <h2 className="text-2xl font-semibold text-white">Welcome back</h2>
              <p className="text-sm text-slate-400">Use your company credentials to continue.</p>
            </div>

            <label className="block text-sm font-medium text-slate-200">
              Email
              <input
                className="mt-2 w-full rounded-lg border border-slate-700 bg-slate-950 px-4 py-2 text-sm text-white outline-none transition focus:border-slate-500"
                placeholder="name@company.com"
                type="email"
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </label>

            <label className="block text-sm font-medium text-slate-200">
              Password
              <input
                type="password"
                className="mt-2 w-full rounded-lg border border-slate-700 bg-slate-950 px-4 py-2 text-sm text-white outline-none transition focus:border-slate-500"
                placeholder="Enter your password"
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </label>

            <div className="flex items-center justify-between text-sm">
              <Link to="/forgot-password" className="text-emerald-400 hover:text-emerald-300">
                Forgot password?
              </Link>
            </div>

            {error && <p className="text-sm text-rose-400">{error}</p>}

            <button
              className="w-full rounded-lg bg-emerald-500 px-4 py-2 text-sm font-semibold text-slate-950 transition hover:bg-emerald-400 disabled:cursor-not-allowed disabled:opacity-60"
              disabled={loading}
            >
              {loading ? "Signing in..." : "Login"}
            </button>

            <p className="text-sm text-slate-400">
              New here?{" "}
              <Link to="/register" className="text-emerald-400 hover:text-emerald-300">
                Create an account
              </Link>
            </p>
          </form>
        </div>
      </div>
    </div>
  );
}
