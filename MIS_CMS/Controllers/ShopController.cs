using MIS_CMS.Models.Data;
using MIS_CMS.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MIS_CMS.Controllers
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
            List<CategoryVM> categoryVMList;
            using (DB db= new DB())
            {

               categoryVMList= db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }



            return PartialView(categoryVMList);
        }

        public ActionResult Category(string name)
        {
            List<ProductVM> productVMlist;

            using (DB db = new DB())
            {
                try
                {
                    CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                    int catId = categoryDTO.Id;

                    var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();

                    if (productCat != null)
                    {
                        productVMlist = db.Products.ToArray().
                        Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();



                        ViewBag.CategoryName = productCat.CategoryName;

                        return View(productVMlist);
                    }


                    
                    return View();

                }
                catch (Exception e)
                {

                    throw new Exception(e.Message);
                }
               

            }
            
        }

        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            ProductVM model;
            ProductDTO dto;

            int id = 0;
            using (DB db= new DB())
            {
                if(!db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }

                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                id = dto.Id;

                model = new ProductVM(dto);


            }

            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs")).Select(f=>Path.GetFileName(f));



            return View("ProductDetails",model);
        }


    }
}