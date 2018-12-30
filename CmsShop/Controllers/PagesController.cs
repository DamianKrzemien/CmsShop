using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Pages;

namespace CmsShop.Controllers
{
    public class PagesController : Controller
    {
        // GET: Index
        [HttpGet]
        public ActionResult Index( string page = "")
        {
            //ustawianie adresu strony
            if (page == "")
            {
                page = "home";
            }
            
            //deklarujemy pageViewModel i pageDTO

            PageViewModel model;
            PageDTO dto;

            //sprawdzamy czy strona istnieje
            using (Db db = new Db())
            {
                if (!db.Pages.Any(x => x.Slug.Equals(page)))
                {
                    return RedirectToAction("Index", new {page = ""});
                }
            }

            //pobieramy pageDTO

            using (Db db = new Db())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }

            //ustawiamy tytuł strony

            ViewBag.PageTitle = dto.Title;

            //sprawdzenie czy strona ma sidebar

            if (dto.HasSidebar == true)
            {
                ViewBag.Sidebar = "Tak";
            }
            else
            {
                ViewBag.Sidebar = "Nie";
            }

            //inicjalizacja pageViewModel

            model = new PageViewModel(dto);

            //zwracamy widok z pageViewModel
            
            return View(model);
        }

        public ActionResult PagesMenuPartial()
        {
            //Deklaracja PageViewModel
            List<PageViewModel> pageViewModelList;
            
            //pobranie stron
            using (Db db = new Db())
            {
                pageViewModelList = db.Pages.ToArray()
                    .OrderBy(x => x.Sorting)
                    .Where(x => x.Slug != "home")
                    .Select(x => new PageViewModel(x))
                    .ToList();
            }

            return PartialView(pageViewModelList);
        }

        public ActionResult SidebarPartial()
        {
            //deklarujemy model
            SidebarViewModel model;
           
            //inicjalizujemy model
            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebar.Find(1);
                model = new SidebarViewModel(dto);
            }

            //zwracamy partial z modelem
            return PartialView(model);
        }


    }

}