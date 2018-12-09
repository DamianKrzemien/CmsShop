using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CmsShop.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        //GET: Admin/Shop/Categories
        [HttpGet]
        public ActionResult Categories()
        {
            //deklaracja listy kategorii do wyświetlenia
            List<CategoryViewModel> categoryViewModelList;

            using (Db db = new Db())
            {
                categoryViewModelList = db.Categories
                                        .ToArray()
                                        .OrderBy(x => x.Sorting)
                                        .Select(x => new CategoryViewModel(x))
                                        .ToList();
            }

            return View(categoryViewModelList);
        }

        //POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //Deklaracja id
            string id;

            using (Db db = new Db())
            {
                //sprawdzenie czy kategoria jest unikalna
                if (db.Categories.Any(x => x.Name == catName))
                {
                    return "tytulzajety";
                }

                //inicjalizacja DTO
                CategoryDTO dto = new CategoryDTO();
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 1000;
                
                //zapis do bazy
                db.Categories.Add(dto);
                db.SaveChanges();

                //pobieramy id
                id = dto.Id.ToString();
            }

            return id;
        }

        //POST: Admin/Shope/ReorderCategories
        [HttpPost]
        public ActionResult ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                // inicjalizacja licznika
                int count = 1;
               
                // deklaracja DTO
                CategoryDTO dto;
                
                //Sortowanie kategorii
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }
            }
            
            return View();
        }

        //GET: Admin/Shop/DeleteCategory
        [HttpGet]
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                //pobieramy kategorię
                CategoryDTO dto = db.Categories.Find(id);

                //usuwanie kategorii
                db.Categories.Remove(dto);

                //zapis na bazie
                db.SaveChanges();

            }
                return RedirectToAction("Categories");
        }


        //POST: Admin/Shop/RenameCategory
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                //sprawdzenie czy kategoria jest unikatowa
                if (db.Categories.Any((x => x.Name == newCatName)))
                {
                    return "tytulzajety";
                }

                //pobranie i edycja kategorii
                CategoryDTO dto = db.Categories.Find(id);

                //edycja kategorii
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                // zapis na bazie
                db.SaveChanges();
            }

            return "OK";
        }
    }
}