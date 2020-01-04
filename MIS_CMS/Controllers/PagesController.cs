using MIS_CMS.Models.Data;
using MIS_CMS.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MIS_CMS.Controllers
{
    public class PagesController : Controller
    {
        // GET: Pages
        public ActionResult Index(string page="")
        {
            if (page == "")
            {
                page = "home";

            }

            PageVM model;
            PageDTO dto;

            using (DB db= new DB())
            {
               if(! db.Pages.Any(x=>x.Slug.Equals(page)))
                {
                    return RedirectToAction("Index",new { page="" });

                }
            }

            using (DB db=new DB())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();

            }

            ViewBag.PageTitle = dto.Title;

            if (dto.HasSidebar==true)
            {
                ViewBag.Sidebar = "Yes";
            }
            else
            {
                ViewBag.Sidebar = "No";
            }

            model = new PageVM(dto);

            return View(model);




           
        }

        //this dynamically renders navbar items in the navbar
        public ActionResult PageMenuPartial()
        {
            List<PageVM> pageList;

            using (DB db= new DB())
            {
                pageList = db.Pages.ToArray().OrderBy(x => x.Sorting).Where(x => x.Slug != "home").Select(x => new PageVM(x)).ToList();

            }


            return PartialView(pageList);
        }

        public ActionResult SidebarPartial()
        {
            SidebarVM model;

            using (DB db = new DB() )
            {
                SidebarDTO dto = db.Sidebar.Find(1);

                model = new SidebarVM(dto);
            }
            


            return PartialView(model);
        }
    }
}