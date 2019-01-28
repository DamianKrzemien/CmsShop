using System.Data.Entity.Migrations.Model;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
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

        //POST: /account/login
        [HttpPost]
        public ActionResult Login(LoginUserViewModel model)
        {
            //sprawdzenie model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //sprawdzamy użytkownika
            bool isValid = false;

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.UserName.Equals(model.UserName) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }

            if (!isValid)
            {
                ModelState.AddModelError("", "Nieprawidłowa nazwa użytkownika bądź hasło");
                return View(model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.UserName, model.RememberMe));
            }
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


        //GET: /account/logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return Redirect("~/account/login");
        }

        public ActionResult UserNavPartial()
        {
            //pobieramy username
            string username = User.Identity.Name;
            
            //deklarujemy model
            UserNavPartialViewModel model;

            using (Db db = new Db())
            {
                //pobieramy użytkownika
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == username);

                model = new UserNavPartialViewModel()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };

            }

            return PartialView(model);
        }

        //GET: /account/userprofile
        [HttpGet]
        [ActionName("user-profile")]
        public ActionResult UserProfile()
        {
            //pobieramy nazwę użytkownika
            string username = User.Identity.Name;

            //deklarujemy model
            UserProfileViewModel model;

            using (Db db = new Db())
            {
                //povieramy użytkownika
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == username);

                model = new UserProfileViewModel(dto);
            }

            return View("UserProfile", model);
        }

        //POST: /account/user-profile
        [HttpPost]
        [ActionName("user-profile")]
        public ActionResult UserProgile(UserProfileViewModel model)
        {
            //sprawdzenie model state
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            //sprawdzamy hasła
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Hasła nie pasują do siebie");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                //pobieramy nazwę użytkownika
                string userName = User.Identity.Name;

                //sprawdzenie czy nazwa jest unikalna
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.UserName == userName))
                {
                    ModelState.AddModelError("", "Nazwa użytkownika " + model.UserName + " zajęta");
                    model.UserName = "";
                    return View("UserProfile", model);
                }

                //edycja dto
                UserDTO dto = db.Users.Find(model.Id);
                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.UserName = model.UserName;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }

                //zapis
                db.SaveChanges();
            }

            //ustawienie komunikatu
            TempData["SM"] = "Edytowałeś swój profil";
            
            return Redirect("~/account/user-profile");
        }
    }
}