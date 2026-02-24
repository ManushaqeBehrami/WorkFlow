import { Routes, Route, Navigate } from "react-router-dom";
import DashboardLayout from "./layouts/DashboardLayouts";
import Login from "./pages/auth/Login";
import Register from "./pages/auth/Register";
import ForgotPassword from "./pages/auth/ForgotPassword";
import DashboardHome from "./pages/dashboard/DashboardHome";
import Payments from "./pages/Payments";
import AuditLogs from "./pages/AuditLogs";
import Employees from "./pages/Employees";
import PTORequest from "./pages/PTORequest";
import DocumentViewer from "./pages/DocumentViewer";
import ProtectedRoute from "./components/ProtectedRoute";

function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route path="/forgot-password" element={<ForgotPassword />} />

      <Route
        path="/"
        element={
          <ProtectedRoute>
            <DashboardLayout />
          </ProtectedRoute>
        }
      >
        <Route index element={<DashboardHome />} />
        <Route
          path="payments"
          element={
            <ProtectedRoute roles={["HR"]}>
              <Payments />
            </ProtectedRoute>
          }
        />
        <Route
          path="employees"
          element={
            <ProtectedRoute roles={["HR"]}>
              <Employees />
            </ProtectedRoute>
          }
        />
        <Route
          path="logs"
          element={
            <ProtectedRoute roles={["HR"]}>
              <AuditLogs />
            </ProtectedRoute>
          }
        />
        <Route
          path="pto"
          element={
            <ProtectedRoute roles={["HR", "Manager", "Employee"]}>
              <PTORequest />
            </ProtectedRoute>
          }
        />
        <Route
          path="documents/:id"
          element={
            <ProtectedRoute roles={["HR", "Manager", "Employee"]}>
              <DocumentViewer />
            </ProtectedRoute>
          }
        />
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default App;
