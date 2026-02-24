using System.Text;

namespace WorkFlow.Services
{
    public class LocalContractFileService
    {
        private readonly IWebHostEnvironment _env;
        private const string SeedContractFileName = "Employment_Contract_2026.pdf";

        public LocalContractFileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string EnsureSeedContractFile()
        {
            var uploadsRoot = Path.Combine(_env.ContentRootPath, "uploads");
            Directory.CreateDirectory(uploadsRoot);

            var fullPath = Path.Combine(uploadsRoot, SeedContractFileName);
            if (!File.Exists(fullPath))
            {
                var pdfBytes = CreateMinimalPdfBytes("WorkFlow Employment Contract (Seed)");
                File.WriteAllBytes(fullPath, pdfBytes);
            }

            return $"/uploads/{SeedContractFileName}";
        }

        private static byte[] CreateMinimalPdfBytes(string contentText)
        {
            var safeText = contentText
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("(", "\\(", StringComparison.Ordinal)
                .Replace(")", "\\)", StringComparison.Ordinal);

            var streamContent = $"BT\n/F1 16 Tf\n36 140 Td\n({safeText}) Tj\nET";

            var objects = new[]
            {
                "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n",
                "2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n",
                "3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>\nendobj\n",
                $"4 0 obj\n<< /Length {Encoding.ASCII.GetByteCount(streamContent)} >>\nstream\n{streamContent}\nendstream\nendobj\n",
                "5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n"
            };

            const string header = "%PDF-1.4\n";
            var offsets = new List<int>();
            var currentOffset = Encoding.ASCII.GetByteCount(header);

            foreach (var obj in objects)
            {
                offsets.Add(currentOffset);
                currentOffset += Encoding.ASCII.GetByteCount(obj);
            }

            var xrefOffset = currentOffset;
            var xrefBuilder = new StringBuilder();
            xrefBuilder.Append("xref\n");
            xrefBuilder.Append($"0 {objects.Length + 1}\n");
            xrefBuilder.Append("0000000000 65535 f \n");
            foreach (var offset in offsets)
            {
                xrefBuilder.Append($"{offset:D10} 00000 n \n");
            }

            var trailer = $"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF\n";
            var pdf = header + string.Concat(objects) + xrefBuilder + trailer;
            return Encoding.ASCII.GetBytes(pdf);
        }
    }
}
