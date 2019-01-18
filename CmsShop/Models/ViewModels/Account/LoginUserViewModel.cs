using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CmsShop.Models.ViewModels.Account
{
    public class LoginUserViewModel
    {
        [Required]
        [Display(Name = "Nazwa użytkownika")]
        public string UserName { get; set; }
        [Required]
        [Display(Name = "Hasło")]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}