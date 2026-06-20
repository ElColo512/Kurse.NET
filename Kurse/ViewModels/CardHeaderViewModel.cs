namespace Kurse.ViewModels
{
    public class CardHeaderViewModel
    {
        public string Title { get; set; } = string.Empty;
        public bool ShowButton { get; set; } = true;
        public string ButtonText { get; set; } = "Nuevo";
        public string ButtonUrl { get; set; } = "#";
    }
}
