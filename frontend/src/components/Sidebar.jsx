import { NavLink } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

const linkBase = "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition";

const NavItem = ({ to, label }) => (
  <NavLink
    to={to}
    className={({ isActive }) =>
      `${linkBase} ${
        isActive
          ? "bg-slate-100 text-slate-900 dark:bg-slate-800 dark:text-white"
          : "text-slate-600 hover:bg-slate-200/40 hover:text-slate-900 dark:text-slate-300 dark:hover:bg-slate-800"
      }`
    }
  >
    {label}
  </NavLink>
);

export default function Sidebar() {
  const { user } = useAuth();

  return (
    <aside className="w-64 shrink-0 border-r border-slate-200/60 bg-slate-50/80 p-6 dark:border-slate-800 dark:bg-slate-950/80">
      <div className="mb-8">
        <p className="text-xs uppercase tracking-[0.2em] text-slate-500 dark:text-slate-400">WorkFlow</p>
        <h2 className="text-xl font-semibold text-slate-900 dark:text-white">People Ops</h2>
      </div>

      <nav className="space-y-2">
        <NavItem to="/" label="Dashboard" />

        {user?.role === "HR" && (
          <>
            <NavItem to="/employees" label="Employees" />
            <NavItem to="/payments" label="Payments" />
            <NavItem to="/logs" label="Audit Logs" />
            <NavItem to="/pto" label="PTO Overview" />
          </>
        )}

        {user?.role === "Manager" && (
          <>
            <NavItem to="/pto" label="Team PTO" />
          </>
        )}

        {user?.role === "Employee" && (
          <>
            <NavItem to="/pto" label="My PTO" />
          </>
        )}
      </nav>
    </aside>
  );
}
