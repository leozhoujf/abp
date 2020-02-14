using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace MyCompanyName.MyProjectName.Web.Pages.Shared.KendoScripts
{
    public class KendoScriptsViewComponent : AbpViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Pages/Shared/KendoScripts/Default.cshtml");
        }
    }
}
