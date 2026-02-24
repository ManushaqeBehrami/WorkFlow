import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { loadStripe } from "@stripe/stripe-js";
import { api } from "../api/axios";

const stripePromise = loadStripe(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY || "");

function PaymentForm({ employees, onPaymentRecorded }) {
  const [employeeId, setEmployeeId] = useState("");
  const [amount, setAmount] = useState("");
  const [status, setStatus] = useState("idle");
  const [error, setError] = useState("");
  const cardMountRef = useRef(null);
  const cardElementRef = useRef(null);
  const stripeRef = useRef(null);

  useEffect(() => {
    let mounted = true;

    const setup = async () => {
      if (!stripeRef.current) {
        stripeRef.current = await stripePromise;
      }
      if (!stripeRef.current || !cardMountRef.current || cardElementRef.current) return;

      const elements = stripeRef.current.elements();
      const card = elements.create("card", {
        style: {
          base: {
            color: "#e2e8f0",
            fontFamily: "Space Grotesk, sans-serif",
            fontSize: "14px",
            "::placeholder": { color: "#64748b" },
          },
        },
      });

      card.mount(cardMountRef.current);
      if (mounted) {
        cardElementRef.current = card;
      }
    };

    setup();

    return () => {
      mounted = false;
      if (cardElementRef.current) {
        cardElementRef.current.destroy();
        cardElementRef.current = null;
      }
    };
  }, []);

  const canSubmit = useMemo(
    () => !!employeeId && Number(amount) > 0 && status !== "processing",
    [employeeId, amount, status]
  );

  const submit = async (e) => {
    e.preventDefault();

    if (!stripeRef.current || !cardElementRef.current) return;

    setStatus("processing");
    setError("");

    try {
      const intent = await api.request("/payments/intent", "POST", {
        userId: Number(employeeId),
        amount: Number(amount),
        currency: "usd",
      });

      const confirmation = await stripeRef.current.confirmCardPayment(intent.clientSecret, {
        payment_method: { card: cardElementRef.current },
      });

      if (confirmation.error) {
        throw new Error(confirmation.error.message);
      }

      const paymentIntentId = confirmation.paymentIntent?.id;
      if (!paymentIntentId) throw new Error("Payment intent confirmation failed.");

      await api.request("/payments/record", "POST", {
        userId: Number(employeeId),
        amount: Number(amount),
        paymentIntentId,
        status: "Paid",
      });

      setEmployeeId("");
      setAmount("");
      cardElementRef.current.clear();
      setStatus("success");
      onPaymentRecorded();
    } catch (err) {
      setError(err.message || "Unable to process payment.");
      setStatus("idle");
    }
  };

  return (
    <form onSubmit={submit} className="grid gap-4 rounded-2xl border border-slate-200/70 bg-white/80 p-5 shadow-sm dark:border-slate-800 dark:bg-slate-900/70 md:grid-cols-[1.2fr_0.7fr_1fr_auto]">
      <select
        className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-700 shadow-sm dark:border-slate-700 dark:bg-slate-950 dark:text-slate-200"
        value={employeeId}
        onChange={(e) => setEmployeeId(e.target.value)}
      >
        <option value="">Select employee</option>
        {employees.map((emp) => (
          <option key={emp.id} value={emp.id}>
            {emp.fullName}
          </option>
        ))}
      </select>

      <input
        className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-700 shadow-sm dark:border-slate-700 dark:bg-slate-950 dark:text-slate-200"
        placeholder="Amount"
        type="number"
        value={amount}
        onChange={(e) => setAmount(e.target.value)}
      />

      <div className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-700 shadow-sm dark:border-slate-700 dark:bg-slate-950 dark:text-slate-200">
        <div ref={cardMountRef} />
      </div>

      <button
        className="rounded-lg bg-emerald-500 px-4 py-2 text-sm font-semibold text-slate-950 hover:bg-emerald-400 disabled:cursor-not-allowed disabled:opacity-60"
        disabled={!canSubmit}
      >
        {status === "processing" ? "Processing..." : "Process payment"}
      </button>

      {error && <p className="text-sm text-rose-400 md:col-span-4">{error}</p>}
      {status === "success" && (
        <p className="text-sm text-emerald-300 md:col-span-4">Payment processed successfully.</p>
      )}
    </form>
  );
}

export default function Payments() {
  const [payments, setPayments] = useState([]);
  const [employees, setEmployees] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const loadData = useCallback(async () => {
    try {
      setLoading(true);
      const [usersData, paymentsData] = await Promise.all([
        api.request("/users"),
        api.request("/payments"),
      ]);
      setEmployees(usersData.filter((u) => u.role !== "HR"));
      setPayments(paymentsData);
    } catch (err) {
      setError(err.message || "Unable to load payments.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const stripeKey = import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">Payments</h1>
        <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">
          HR can issue payments and monitor payroll status.
        </p>
      </div>

      {!stripeKey && (
        <div className="rounded-2xl border border-amber-300 bg-amber-50 px-4 py-3 text-sm text-amber-700 dark:border-amber-500/40 dark:bg-amber-500/10 dark:text-amber-200">
          Stripe is not configured. Add `VITE_STRIPE_PUBLISHABLE_KEY` to enable payments.
        </div>
      )}

      {stripeKey && (
        <PaymentForm employees={employees} onPaymentRecorded={loadData} />
      )}

      {loading && <p className="text-sm text-slate-400">Loading payments...</p>}
      {error && <p className="text-sm text-rose-400">{error}</p>}

      <div className="space-y-3">
        {payments.map((pay) => (
          <div key={pay.id} className="flex flex-wrap items-center justify-between gap-4 rounded-2xl border border-slate-200/70 bg-white/80 px-4 py-3 text-sm shadow-sm dark:border-slate-800 dark:bg-slate-900/70">
            <div>
              <p className="font-medium text-slate-900 dark:text-white">{pay.userFullName}</p>
              <p className="text-xs text-slate-500 dark:text-slate-400">{new Date(pay.paymentDate).toLocaleDateString()}</p>
            </div>
            <div className="flex items-center gap-4">
              <span className="text-base font-semibold text-slate-900 dark:text-white">${Number(pay.amount).toLocaleString()}</span>
              <span className={`rounded-full px-3 py-1 text-xs font-semibold ${
                pay.status === "Paid"
                  ? "bg-emerald-100 text-emerald-700 dark:bg-emerald-400/20 dark:text-emerald-200"
                  : pay.status === "Failed"
                  ? "bg-rose-100 text-rose-700 dark:bg-rose-400/20 dark:text-rose-200"
                  : "bg-amber-100 text-amber-700 dark:bg-amber-400/20 dark:text-amber-200"
              }`}>
                {pay.status}
              </span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
