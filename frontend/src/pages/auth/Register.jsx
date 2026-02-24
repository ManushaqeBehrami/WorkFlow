import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { api } from "../../api/axios";

export default function Register() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const submit = async (e) => {
    e.preventDefault();
    setError("");

    if (password !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    setLoading(true);

    try {
      await api.request("/auth/register", "POST", { email, password });
      navigate("/login");
    } catch (err) {
      setError(err.message || "Registration failed");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100">
      <div className="mx-auto flex min-h-screen max-w-4xl items-center px-6">
        <form onSubmit={submit} className="w-full space-y-5 rounded-2xl border border-slate-800 bg-slate-900/80 p-8">
          <div>
            <p className="text-xs uppercase tracking-[0.25em] text-slate-400">WorkFlow</p>
            <h2 className="mt-2 text-2xl font-semibold text-white">Create your account</h2>
            <p className="text-sm text-slate-400">We will route you to login once you are registered.</p>
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
              placeholder="Create a password"
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </label>

          <label className="block text-sm font-medium text-slate-200">
            Confirm password
            <input
              type="password"
              className="mt-2 w-full rounded-lg border border-slate-700 bg-slate-950 px-4 py-2 text-sm text-white outline-none transition focus:border-slate-500"
              placeholder="Repeat your password"
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
            />
          </label>

          {error && <p className="text-sm text-rose-400">{error}</p>}

          <button
            className="w-full rounded-lg bg-emerald-500 px-4 py-2 text-sm font-semibold text-slate-950 transition hover:bg-emerald-400 disabled:cursor-not-allowed disabled:opacity-60"
            disabled={loading}
          >
            {loading ? "Creating account..." : "Register"}
          </button>

          <p className="text-sm text-slate-400">
            Already have an account?{" "}
            <Link to="/login" className="text-emerald-400 hover:text-emerald-300">
              Sign in
            </Link>
          </p>
        </form>
      </div>
    </div>
  );
}
