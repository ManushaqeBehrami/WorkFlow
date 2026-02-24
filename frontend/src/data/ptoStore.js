const STORAGE_KEY = "workflow_pto_requests_v1";

const seedRequests = [
  { id: 1, employee: "maya.johnson@company.com", dates: "Mar 4 - Mar 5", reason: "Family event", status: "Pending" },
  { id: 2, employee: "carlos.ruiz@company.com", dates: "Mar 12 - Mar 14", reason: "Medical", status: "Approved" },
  { id: 3, employee: "priya.singh@company.com", dates: "Mar 20 - Mar 21", reason: "Travel", status: "Pending" },
];

export const loadPtoRequests = () => {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) return seedRequests;
  try {
    const parsed = JSON.parse(raw);
    return Array.isArray(parsed) ? parsed : seedRequests;
  } catch {
    return seedRequests;
  }
};

export const savePtoRequests = (requests) => {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(requests));
};

export const resetPtoRequests = () => {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(seedRequests));
  return seedRequests;
};
