using CmsShop.Models.Data;
using System.Web.Mvc;

namespace CmsShop.Models.ViewModels.Pages
{
    public class SidebarViewModel
    {
        public SidebarViewModel()
        {
            
        }

        public SidebarViewModel(SidebarDto row)
        {
            Id = row.Id;
            Body = row.Body;
        }
        
        public int Id { get; set; }
        [AllowHtml]
        public string Body { get; set; }
    }
}