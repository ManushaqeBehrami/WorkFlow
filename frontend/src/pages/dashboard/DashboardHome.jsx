import { useAuth } from "../../context/AuthContext";
import HRDashboard from "./HRDashboard";
import ManagerDashboard from "./ManagerDashboard";
import EmployeeDashboard from "./EmployeeDashboard";

export default function DashboardHome() {
  const { user } = useAuth();

  if (user?.role === "HR") return <HRDashboard />;
  if (user?.role === "Manager") return <ManagerDashboard />;
  return <EmployeeDashboard />;
}
