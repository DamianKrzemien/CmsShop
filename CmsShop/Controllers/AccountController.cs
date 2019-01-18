using System.Linq;
using System.Web.Mvc;
using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Account;

namespace CmsShop.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }

        //GET: /account/login
        public ActionResult Login()
        {
            //sprawdzenie czy użytkownik nie jest już zalogowany
            string userName = User.Identity.Name;
            if (!string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("user-profile");
            }

            return View();
        }


        //GET: /account/create-account
        [ActionName("create-account")]
        public ActionResult CreateAccount()
        {

            return View("CreateAccount");
        }


        //POST: /account/create-account
        [HttpPost]
        [ActionName("create-account")]
        public ActionResult CreateAccount(UserViewModel model)
        {
            //Sprawdzenie model state
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }

            //sprawdzenie hasła
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Hasła nie pasują do siebie");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                //Sprawdzenie czy nazwa jest unikalna
                if (db.Users.Any(x => x.UserName.Equals(model.UserName)))
                {
                    ModelState.AddModelError("", "Nazwa użytkownika " + model.UserName + " jest już zajęta");
                    return View("CreateAccount", model);
                }

                //utworzenie użytkownika
                UserDTO usersDto = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    UserName = model.UserName,
                    Password = model.Password
                };

                //dodanie użytkownika
                db.Users.Add(usersDto);
                db.SaveChanges();

                //dodanie roli dla użytkownika
                UserRoleDTO userRoleDto = new UserRoleDTO()
                {
                    UserId = usersDto.Id,
                    RoleId = 2
                };

                //dodanie roli
                db.UserRole.Add(userRoleDto);
                db.SaveChanges();
            }

            //Tempdata komunikat
            TempData["SM"] = "Jesteś teraz zarejestrowany i możesz się zalogować";

            return Redirect("~/account/login");
        }


    }
}