using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Pages;

namespace CmsShop.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //Deklaracja listy PageViewModel
            List<PageViewModel> pagesList;

            //Inicjalizacja listy
            using (Db db = new Db())
            {
                pagesList = db.Pages.ToArray()
                    .OrderBy(x => x.Sorting)
                    .Select(x => new PageViewModel(x))
                    .ToList();
            }

            // zwracamy strony do widoku
            return View(pagesList);
        }

        //GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {

            return View();
        }
        //POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageViewModel model)
        {
            //Sprawdzanie model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                string slug;

                //Inicjaclizacja PageDTO
                PageDTO dto= new PageDTO();

                
                //jeżeli slug jest pusty to przypisujemy tytuł
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                //zapobiegamy dodaniu takiej samej nazwy strony
                if (db.Pages.Any(x => x.Title == model.Title ) || db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "Ten tytuł lub adres już istnieje");
                    return View(model);
                }

                dto.Title = model.Title;
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 1000;

                //zapis dto w bazie danych
                db.Pages.Add(dto);
                db.SaveChanges();
            }

            TempData["SM"] = "Dodałeś nową stronę";

            return RedirectToAction("AddPage");
        }
    }
}