using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CmsShop.Models.Data;
using CmsShop.Views.Cart;

namespace CmsShop.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            //inicjalizacja koszyka
            var cart = Session["cart"] as List<CartViewModel> ?? new List<CartViewModel>();

            //sprawdzamy czy koszyk jest pusty
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Twój koszyk jest pusty";
                return View();
            }

            //obliczenie podsumowania koszyka i przekazanie do viewbag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            return View(cart);
        }

        public ActionResult CartPartial()
        {
            //inicjalizacja CartViewModel
            CartViewModel model = new CartViewModel();

            //inicjalizacja ilości i ceny
            int qty = 0;
            decimal price = 0;

            //sprawdzamy czy mamy dane koszyka zapisane w sesji
            if (Session["cart"] != null)
            {
                //pobieranie wartości z sesji
                var list = (List<CartViewModel>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Total;
                }

                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                qty = 0;
                price = 0m;
            }

            return PartialView(model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            //Inicjalizacja CartViewModel List
            List<CartViewModel> cart = Session["cart"] as List<CartViewModel> ?? new List<CartViewModel>();

            //Inicjalizacja CartViewModel
            CartViewModel model = new CartViewModel();

            using (Db db = new Db())
            {
                //pobieramy produkt
                ProductDTO product = db.Products.Find(id);

                //sprawdzamy czy ten produkt jest już w koszyku
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                //w zależności od tego czy produkt jest w koszyku dodajemy lub zwiększamy liczbę
                if (productInCart == null)
                {
                    cart.Add(new CartViewModel()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }
                else
                {
                    productInCart.Quantity++;
                }
            }
            //pobieramy wartość ilości i ceny i dodajemy do modelu
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            //zapis w sesji
            Session["cart"] = cart;

            return PartialView(model);
        }

        public JsonResult IncrementProduct(int productId)
        {
            // Inicjalizacja listy CartViewModel
            List<CartViewModel> cart = Session["cart"] as List<CartViewModel>;

            //Pobieramy cartViewModel
            CartViewModel model = cart.FirstOrDefault(x => x.ProductId == productId);

            model.Quantity++;

            //przygotowanie danych do Jsona
            var result = new {qty = model.Quantity, price = model.Price};

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DecrementProduct(int productId)
        {

            // Inicjalizacja listy CartViewModel
            List<CartViewModel> cart = Session["cart"] as List<CartViewModel>;

            //Pobieramy cartViewModel
            CartViewModel model = cart.FirstOrDefault(x => x.ProductId == productId);

            if (model.Quantity > 1)
            {
                model.Quantity--;
            }
            else
            {
                model.Quantity = 0;
                cart.Remove(model);
            }

            //przygotowanie danych do Jsona
            var result = new { qty = model.Quantity, price = model.Price };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public void RemoveProduct(int productId)
        {
            List<CartViewModel> cart = Session["cart"] as List<CartViewModel>;

            CartViewModel  model = cart.FirstOrDefault(x => x.ProductId == productId);

            cart.Remove(model);
        }
    }
}