using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;

namespace CmsShop.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }
        
        public ActionResult CategoryMenuPartial()
        {
            //deklarujemy CategoryViewModel List
            List<CategoryViewModel> categoryViewModelList;

            //inicjalizacja listy
            using (Db db = new Db())
            {
                categoryViewModelList = db.Categories
                    .ToArray()
                    .OrderBy(x => x.Sorting)
                    .Select(x => new CategoryViewModel(x))
                    .ToList();
            }

            return PartialView(categoryViewModelList);
        }

        //Get: shop/category/name
        [HttpGet]
        public ActionResult Category(string name)
        {
            //deklaracja productVieModelList
            List<ProductViewModel> productViewModelList;

            using (Db db = new Db())
            {
                //pobranie id kategorii
                CategoryDTO categoryDto = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categoryDto.Id;

                //inicjalizacja listy produktów
                productViewModelList = db.Products
                    .ToArray()
                    .Where(x => x.CategoryId == catId)
                    .Select(x => new ProductViewModel(x))
                    .ToList();

                //pobranie nazwy kategorii
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();
                ViewBag.CategoryName = productCat.CategoryName;

            }

            return View(productViewModelList);
        }

        //GET: /shop/product-szczegoly/name
        [ActionName("product-szczegoly")]
        [HttpGet]
        public ActionResult ProductDetails(string name)
        {
            //deklaracja Product VM i productDTO
            ProductViewModel model;
            ProductDTO dto;

            //inicjalizacja product id
            int id = 0;

            using (Db db = new Db())
            {
                //sprawdzamy czy produkt istnieje
                if (!db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }

                //inicjalizacja productDTO
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                //pobranie id
                id = dto.Id;

                //inicjalizacja modelu
                model = new ProductViewModel(dto);

            }

            //pobieramy galerię zdjęć do produktu
            model.GalleryImages =
                Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Galery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));


            return View("ProductDetails", model);
        }
    }
}