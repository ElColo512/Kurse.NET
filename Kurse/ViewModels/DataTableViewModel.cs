namespace Kurse.ViewModels
{
    public class DataTableViewModel
    {
        public string TableId { get; set; } = string.Empty;
        public List<string> Columns { get; set; } = [];
        public bool ShowActions { get; set; }
    }
}
