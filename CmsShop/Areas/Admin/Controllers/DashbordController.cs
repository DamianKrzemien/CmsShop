using System.Web.Mvc;

namespace CmsShop.Areas.Admin.Controllers
{
    public class DashbordController : Controller
    {
        // GET: Admin/Dashbord
        public ActionResult Index()
        {
            return View();
        }
    }
}