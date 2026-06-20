using Kurse.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Kurse.ViewComponents
{
    public class DataTableViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(DataTableViewModel model)
        {
            return View(model);
        }
    }
}
