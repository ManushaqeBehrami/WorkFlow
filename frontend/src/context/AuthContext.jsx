import { createContext, useContext, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";

const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

export const AuthContext = createContext(null);

const decodeBase64Url = (input) => {
  if (!input) return null;
  const base64 = input.replace(/-/g, "+").replace(/_/g, "/");
  const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), "=");
  try {
    return atob(padded);
  } catch {
    return null;
  }
};

const decodeToken = (token) => {
  try {
    const part = token.split(".")[1];
    const decoded = decodeBase64Url(part);
    if (!decoded) return null;
    return JSON.parse(decoded);
  } catch {
    return null;
  }
};

const buildUserFromToken = (token) => {
  if (!token) return null;
  const payload = decodeToken(token);
  if (!payload) return null;

  const role = payload[ROLE_CLAIM] || payload.role || payload.Role || payload.roles?.[0];
  const email = payload.email || payload.unique_name || payload.sub || "unknown@local";

  return {
    role: role || "Employee",
    email,
    token,
    raw: payload,
  };
};

export const AuthProvider = ({ children }) => {
  const navigate = useNavigate();
  const [user, setUser] = useState(() => buildUserFromToken(localStorage.getItem("token")));

  const login = (data) => {
    const token = data?.accessToken || data?.token || data;
    if (!token) {
      throw new Error("Login response did not include a token.");
    }
    localStorage.setItem("token", token);
    setUser(buildUserFromToken(token));
    navigate("/", { replace: true });
  };

  const logout = () => {
    localStorage.removeItem("token");
    setUser(null);
    navigate("/login", { replace: true });
  };

  const value = useMemo(() => ({ user, login, logout }), [user]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => useContext(AuthContext);
