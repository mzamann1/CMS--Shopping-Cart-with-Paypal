using MIS_CMS.Models.Data;
using MIS_CMS.Models.ViewModels.Pages;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

namespace MIS_CMS.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //Declare List of ViewModels
            List<PageVM> pagesList;
            using (DB db = new DB() )
            {
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }
            return View(pagesList);
        }
        //Get :/Admin/Pages/AddPage

        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }


        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (DB db= new DB())
            {
                string slug;

                PageDTO dto = new PageDTO();
                dto.Title = model.Title;
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug= model.Slug;
                }

                if (db.Pages.Any(x=>x.Title==dto.Title)|| db.Pages.Any(x=> x.Slug == slug))
                {
                    ModelState.AddModelError("", "Title/Slug Already Exists");
                    return View(model);
                }
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;

                db.Pages.Add(dto);
                db.SaveChanges();
            }
            TempData["SM"] = "Success Fully Added";
            return RedirectToAction("Index");
        }


        
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            PageVM model = new PageVM();

            using (DB db = new DB())
            {
                PageDTO dto = db.Pages.Find(id);

                if (dto == null)
                {
                    return Content("Page Doesn't Exists...");
                }

                model = new PageVM(dto);

            }

            return View(model);
        }


        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (DB db= new DB())
            {
                int id = model.Id;
                string slug="home";
                PageDTO dto= db.Pages.Find(id);
                dto.Title = model.Title;
                if (model.Slug!="home")
                {
                  if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }

                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) || db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "The Title or Slug Already Exists");
                    return View(model);
                }

                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;

                db.SaveChanges();
            }
            TempData["SM"] = "Success!";

            return RedirectToAction("Index");
        }

       
        [HttpGet]
        public ActionResult DetailsPage(int id)
        {
            PageVM model;
            using (DB db= new DB())
            {
                PageDTO dto;

                dto=db.Pages.Find(id);

                if (dto ==null)
                {
                    return Content("Cannot Show Detail/Page Doesn't Exists... ");
                }

                model = new PageVM(dto);

               
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult DeletePage(int id)
        {
            using (DB db = new DB())
            {
                PageDTO dto;
                dto=db.Pages.Find(id);

                if (dto == null)
                {
                    TempData["SM"] = "Failed!";
                    return Content("Cannot Delete");
                }

                db.Pages.Remove(dto);
                db.SaveChanges();
                TempData["SM"] = "Success Fully Deleted";

                return RedirectToAction("Index");
            }
         
        }

        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (DB db = new DB())
            {
                int count = 1;
                PageDTO dto;

                foreach (var pageId in id)
                {
                  dto=  db.Pages.Find(pageId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }
            }
           
        }

        [HttpGet]
        public ActionResult EditSidebar()
        {
            SidebarVM model;

            using (DB db = new DB())
            {
                SidebarDTO dto = db.Sidebar.Find(1);

                if (dto == null)
                {
                    return Content("404");
                }
                model = new SidebarVM(dto);

            }
            return View(model);
        }

        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            
            using (DB db = new DB())
            {
                SidebarDTO dto;
                dto= db.Sidebar.Find(1);

                if (dto==null)
                {
                    return Content("404");
                }

                dto.Body = model.Body;
                db.SaveChanges();
            }

            TempData["SM"] = "Success!";

            return RedirectToAction("EditSidebar");
        }
    }
}