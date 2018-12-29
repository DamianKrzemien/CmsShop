using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using PagedList;

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

        //POST: Admin/Shop/ReorderCategories
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

        //GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            //Inicjalizacja model
            ProductViewModel model = new ProductViewModel();

            //pobieramy listę kategorii
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

            }

            return View(model);
        }

        //POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductViewModel model, HttpPostedFileBase file)
        {
            //sprawdzamy model state
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            //sprawdzenie czy nazwa produktu jest unikatowa
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "Ta nazwa produktu jest zajęta!");
                    return View(model);
                }
            }

            //deklaracja product id
            int id;

            //dodawanie produktu i zapis na bazie
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);

                product.CategoryName = catDto.Name;

                db.Products.Add(product);
                db.SaveChanges();

                //pobranie id dodanego produktu
                id = product.Id;

            }

            //ustawiamy komunikat 
            TempData["SM"] = "Dodałeś produkt";

            #region Upload Image

            //Utworzenie potrzebnej stutury katalogów
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Galery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Galery\\Thumbs");

            if (!Directory.Exists(pathString1))
            {
                Directory.CreateDirectory(pathString1);
            }

            if (!Directory.Exists(pathString2))
            {
                Directory.CreateDirectory(pathString2);
            }

            if (!Directory.Exists(pathString3))
            {
                Directory.CreateDirectory(pathString3);
            }

            if (!Directory.Exists(pathString4))
            {
                Directory.CreateDirectory(pathString4);
            }

            if (!Directory.Exists(pathString5))
            {
                Directory.CreateDirectory(pathString5);
            }

            if (file != null && file.ContentLength > 0)
            {
                //sprawdzenie rozszerzenia pliku czy mamy do czynienia z obrazkiem
                string ext = file.ContentType.ToLower();

                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "Obraz nie został przesłany - nieprawidłowe rozszerzenie obrazu.");
                        return View(model);
                    }

                }

                //Inicjalizacja nazwy obrazka
                string imageName = file.FileName;

                //zapis nazwy obrazka do bazy
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);

                //zapisujemy oryginalny obrazek
                file.SaveAs(path);

                //zapisujemy miniaturkę

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);

            }
            #endregion

           return RedirectToAction("AddProduct");

        }

        //GET: Admin/Shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            //Deklaracja listy Produktów
            List<ProductViewModel> listOfProductViewModel;

            //ustawiamy numer strony
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                //inicjalizacja listy produktów
                listOfProductViewModel = db.Products.ToArray()
                                           .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                           .Select(x => new ProductViewModel(x))
                                           .ToList();

                //lista kategorii do dropdownList
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //ustawiamy wybraną kategorię
                ViewBag.SelectedCat = catId.ToString();
                
            }

            //ustawienie stronnicowania

            var onePageOfProducts = listOfProductViewModel.ToPagedList(pageNumber, 3);
            ViewBag.OnePageofProducts = onePageOfProducts;
            
            //zwracamy widok z listą produktów
            return View(listOfProductViewModel);

        }

        //GET: Admin/Shop/EditProduct
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            //deklaracja productViewModel
            ProductViewModel model;

            using (Db db = new Db())
            {
                //pobieramy produkt do edycji
                ProductDTO dto = db.Products.Find(id);

                //sprawdzenie czy produkt istnieje
                if (dto == null)
                {
                    return Content("Ten produkt nie istnieje");
                }

                //inicjalizacja modelu
                model = new ProductViewModel(dto);

                //lista kategorii
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //ustawianie zdjęcia
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Galery/Thumbs"))
                                               .Select(fn => Path.GetFileName(fn));
            }   

            return View(model);

        }

        //POST: Admin/Shop/EditProduct
        [HttpPost]
        public ActionResult EditProduct(ProductViewModel model, HttpPostedFileBase file)
        {
            //pobranie id produku
            int id = model.Id;

            //pobranie kategorii dla dropdownLis
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

            }

            //ustawianie zdjęcia
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Galery/Thumbs"))
                                           .Select(fn => Path.GetFileName(fn));

            //sprawdzamy model state

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //sprawdzamy czy nazwa jest unikatowa
            using (Db db = new Db())
            {
                if(db.Products.Where(x => x.Id != id).Any(x => x.Name ==model.Name))
                {
                    ModelState.AddModelError("", "Ta nazwa produktu jest zajęta");
                    return View(model);
                }
            }
            
            //edycja produktu i zapis na bazie
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;
                dto.Description = model.Description;

                CategoryDTO catDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDto.Name;

                db.SaveChanges();
            }

            //ustawienie TempData
            TempData["SM"] = "Edytowano produkt";

            #region Image Upload
            
            //sprawdzamy czy jest plik
            if (file != null && file.ContentLength > 0)
            {
                //sprawdzamy rozszerzenie pliku czy to jest obrazek
                string ext = file.ContentType.ToLower();

                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "Obraz nie został przesłany - nieprawidłowe rozszerzenie obrazu.");
                        return View(model);
                    }
                }

                //Utworzenie potrzebnej stutury katalogów
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            
                //Usuwamy pliki z katalogów
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (var file1 in di1.GetFiles())
                {
                    file1.Delete();
                }

                foreach (var file2 in di2.GetFiles())
                {
                    file2.Delete();
                }

                //zapisujemy nazwę obrazka
                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                //zapis nowych plików
                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);

                file.SaveAs(path);

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);

            }


            #endregion

            return RedirectToAction("EditProduct");
        }
        
        //GET: Admin/Shop/DeketeProduct/id
        [HttpGet]
        public ActionResult DeleteProduct(int id)
        {
            //usunięcie produktu z bazy

            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();
            }

            //usunięcie folderu produktu z wszystkimi plikami
            var orginalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            var pathString = Path.Combine(orginalDirectory.ToString(), "Products" + id.ToString());

            if (Directory.Exists(pathString))
            {
                Directory.Delete(pathString, true);
            }

            return RedirectToAction("Products");

        }

        //Post: Admin/Shop/SaveGalleryImages/id
        [HttpPost]
        public ActionResult SaveGalleryImages(int id)
        {
            //pętla po obrazkach
            foreach (string fileName in Request.Files)
            {
                //Inicjalizacja
                HttpPostedFileBase file = Request.Files[fileName];

                //sprawdzenie czy jest plik
                if (file != null && file.ContentLength > 0)
                {
                    //ustawiamy ścieżki
                    var orginalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                    string pathString1 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Galery");
                    string pathString2 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Galery\\Thumbs");

                    var path = string.Format("{0}\\{1}", pathString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);

                    //zapis obrazków i miniaturek
                    file.SaveAs(path);
                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);
                }

            }

            return View();
        }

        //Post: Admin/Shop/DeleteImage
        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Galery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Galery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
            {
                System.IO.File.Delete(fullPath1);
            }

            if (System.IO.File.Exists(fullPath2))
            {
                System.IO.File.Delete(fullPath2);
            }

        }
    }
}