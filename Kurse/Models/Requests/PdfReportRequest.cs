using Kurse.Models.Enums;

namespace Kurse.Models.Requests
{
    public class PdfReportRequest
    {
        public string Title { get; set; } = string.Empty;
        public List<string> Headers { get; set; } = [];
        public List<List<string>> Rows { get; set; } = [];
        public bool Landscape { get; set; }
        public List<float>? ColumnWidths { get; set; }
        public List<PdfColumnAlignment>? Alignments { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }
}
