const API_BASE = "https://localhost:7190/api";

export const api = {
  async request(endpoint, method = "GET", body) {
    const token = localStorage.getItem("token");
    const res = await fetch(`${API_BASE}${endpoint}`, {
      method,
      headers: {
        "Content-Type": "application/json",
        ...(token && { Authorization: `Bearer ${token}` }),
      },
      body: body ? JSON.stringify(body) : undefined,
    });

    if (!res.ok) {
      const errorData = await res.json().catch(() => ({}));
      throw new Error(errorData.message || "API error");
    }

    if (res.status === 204) return null;
    return res.json();
  },
};

export const apiRequest = (endpoint, method, body) => api.request(endpoint, method, body);
