import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { api } from "../api/axios";

export default function DocumentViewer() {
  const { id } = useParams();
  const [doc, setDoc] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    api
      .request(`/documents/${id}`)
      .then(setDoc)
      .catch((err) => setError(err.message || "Unable to load document."));
  }, [id]);

  if (error) {
    return <p className="text-sm text-rose-400">{error}</p>;
  }

  if (!doc) {
    return <p className="text-sm text-slate-400">Loading document...</p>;
  }

  if (!doc.fileUrl) {
    return <p className="text-sm text-slate-400">No document URL available.</p>;
  }

  const isPdf = doc.fileType?.toLowerCase().includes("pdf") || doc.fileUrl.toLowerCase().endsWith(".pdf");

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-2xl font-semibold">{doc.fileName}</h1>
        <p className="text-sm text-slate-500 dark:text-slate-400">
          {doc.fileType || "Document"}
        </p>
      </div>

      {isPdf ? (
        <div className="h-[75vh] w-full overflow-hidden rounded-2xl border border-slate-200/70 bg-white dark:border-slate-800 dark:bg-slate-900/70">
          <iframe
            title={doc.fileName}
            src={doc.fileUrl}
            className="h-full w-full"
          />
        </div>
      ) : (
        <div className="rounded-2xl border border-slate-200/70 bg-white p-6 text-sm text-slate-600 dark:border-slate-800 dark:bg-slate-900/70 dark:text-slate-300">
          This document cannot be previewed. Use the direct link below.
          <div className="mt-4">
            <a className="text-emerald-500 hover:text-emerald-400" href={doc.fileUrl} target="_blank" rel="noreferrer">
              Open document
            </a>
          </div>
        </div>
      )}
    </div>
  );
}
