using MIS_CMS.Areas.Admin.Models.ViewModels.Shop;
using MIS_CMS.Models.Data;
using MIS_CMS.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace MIS_CMS.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        public ActionResult Categories()
        {
            List<CategoryVM> categoryVMList;

            using (DB db = new DB())
            {
                categoryVMList = db.Categories.ToArray()
                    .OrderBy(x => x.Sorting)
                    .Select(x => new CategoryVM(x))
                    .ToList();
            }

            return View(categoryVMList);
        }

        [HttpPost]
        public string AddCategory(string catName)
        {
            string id;

            using (DB db = new DB())
            {
                if (db.Categories.Any(x => x.Name == catName))
                {
                    return "titletaken";
                }

                CategoryDTO dto = new CategoryDTO();
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();

                dto.Sorting = 100;

                db.Categories.Add(dto);
                db.SaveChanges();

                id = dto.Id.ToString();


            }
            return id;



        }

        [HttpGet]
        public ActionResult DeleteCategory(int id)
        {
            using (DB db = new DB())
            {
                CategoryDTO dto;
                dto = db.Categories.Find(id);

                if (dto == null)
                {
                    TempData["SM"] = "Failed!";
                    return Content("Cannot Delete");
                }

                db.Categories.Remove(dto);
                db.SaveChanges();
                TempData["SM"] = "Success Fully Deleted";

                return RedirectToAction("Categories");
            }

        }

        [HttpPost]
        public void ReorderCategory(int[] id)
        {
            using (DB db = new DB())
            {
                int count = 1;
                CategoryDTO dto;

                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }
            }

        }

        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (DB db = new DB())
            {
                if (db.Categories.Any(x => x.Name == newCatName))
                {
                    return "titletaken";
                }

                CategoryDTO dto = db.Categories.Find(id);

                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                db.SaveChanges();
            }

            return "ok";



        }

        [HttpGet]
        public ActionResult AddProduct()
        {
            ProductVM model = new ProductVM();

            using (DB db = new DB())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");



            }


            return View(model);

        }

        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            //if modelstate isnot valid
            if (!ModelState.IsValid)
            {
                using (DB db = new DB())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }

            }

            //make sure product name is unique
            using (DB db = new DB())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "Product Name is already taken");
                    return View(model);

                }
            }

            //declaring product Id
            int id;

            //init and save Product using DTO
            using (DB db = new DB())
            {
                ProductDTO product = new ProductDTO();
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-");
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);

                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                //get id

                id = product.Id;
            }

            TempData["SM"] = "Product Added Successfully!!";

            #region UploadImage

            //creating directory

            var orignDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads\\", Server.MapPath(@"\")));
            //check if file was uploaded successfully

            var pathString1 = Path.Combine(orignDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(orignDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(orignDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(orignDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(orignDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

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
                //get file extension
                string ext = file.ContentType.ToLower();


                //verify extesion
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/png" &&
                        ext != "image/x-png")
                {
                    using (DB db = new DB())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError(" ", "Image is not in Correct Format!!!");




                    }

                }

                //init image name
                string imageName = file.FileName;

                //Save image
                using (DB db = new DB())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }



                //set original and thumb image paths
                var path = string.Format("{0}\\{1}", pathString2, imageName);

                var path2 = string.Format("{0}\\{1}", pathString3, imageName);
                //save original
                file.SaveAs(path);
                //create and save thumb
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }




            #endregion




            return RedirectToAction("Products");
        }


        public ActionResult Products(int? page, int? catId)
        {

            //Declare a list of VM
            List<ProductVM> _list;
            //Set page num
            var pageNumber = page ?? 1;

            using (DB db = new DB())
            {
                //init the list
                _list = db.Products.ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();

                //populate categories select list
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                //set selected category
                ViewBag.SelectedCat = catId.ToString();
                //return view with list

            }

            var onePageOfProducts = _list.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfProducts = onePageOfProducts;




            return View(_list);
        }

        [HttpGet]

        public ActionResult EditProduct(int id)
        {
            ProductVM model;

            using (DB db = new DB())
            {
                ProductDTO dto;
                dto = db.Products.Find(id);

                if (dto == null)
                {
                    return Content("Not Found!!");
                }

                model = new ProductVM(dto);

                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(x => Path.GetFileName(x));


            }

            return View(model);
        }


        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            //getting product id
            int id = model.Id;


            //populate categories select list and gallery image
            using (DB db = new DB())
            {
                model.Categories = new SelectList(db.Categories, "Id", "Name");




            }
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                 .Select(x => Path.GetFileName(x));




            //model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (DB db = new DB())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "The PRoduct name is taken!");
                    return View(model);
                }
            }
            //update product
            using (DB db = new DB())
            {
                ProductDTO dto = db.Products.Find(id);
                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;

                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO cat = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);

                dto.CategoryName = cat.Name;
                db.SaveChanges();

            }

            TempData["SM"] = "Updated Succesfully!!";

            #region UploadImage

            if (file != null && file.ContentLength > 0)
            {

                string ext = file.ContentType.ToLower();

                //verify extesion
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/png" &&
                        ext != "image/x-png")
                {
                    using (DB db = new DB())
                    {

                        ModelState.AddModelError(" ", "Image is not in Correct Format!!!");
                        return View(model);

                    }


                }

                var orignDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads\\", Server.MapPath(@"\")));


                var pathString1 = Path.Combine(orignDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(orignDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");


                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (FileInfo fileInfo in di1.GetFiles())
                {
                    fileInfo.Delete();

                }

                foreach (FileInfo fileInfo in di2.GetFiles())
                {
                    fileInfo.Delete();

                }

                string imageName = file.FileName;

                using (DB db = new DB())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();

                }

                var path = string.Format("{0}\\{1}", pathString1, imageName);

                var path2 = string.Format("{0}\\{1}", pathString2, imageName);

                file.SaveAs(path);
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);

            }

            #endregion



            return RedirectToAction("Products");

        }

        [HttpGet]
        public ActionResult DeleteProduct(int id)
        {
            ///delete product from DB
            using (DB db = new DB())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();

            }


            ///deleteproduct folder
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads\\", Server.MapPath(@"\")));

            string pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
            {
                Directory.Delete(pathString, true);
            }


            TempData["SM"] = "Deleted Successfully!!";
            return RedirectToAction("Products");
        }

        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            // Loop through files
            foreach (string fileName in Request.Files)
            {
                // Init the file
                HttpPostedFileBase file = Request.Files[fileName];

                // Check it's not null
                if (file != null && file.ContentLength > 0)
                {
                    // Set directory paths
                    var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    // Set image paths
                    var path = string.Format("{0}\\{1}", pathString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);

                    // Save original and thumb

                    file.SaveAs(path);
                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);
                }

            }


        }
        [HttpPost]
        public ActionResult DeleteImage(int id,string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);

            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs" + imageName);

            if (System.IO.File.Exists(fullPath1))
            {
                System.IO.File.Delete(fullPath1);
            }
            if (System.IO.File.Exists(fullPath2))
            {
                System.IO.File.Delete(fullPath2);
            }



            return View();
        }

        public ActionResult Orders()
        {
            List<OrderForAdminVM> orderForAdminVMs = new List<OrderForAdminVM>();
            using (DB db= new DB())
            {
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                foreach (var order in orders)
                {
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    decimal total = 0m;

                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    UserDTO user = db.Users.Where(x => x.Id == order.UserId).FirstOrDefault();

                    string username = user.UserName;


                    foreach (var orderDetails in orderDetailsList)
                    {
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();


                        decimal price = product.Price;

                        string productName = product.Name;

                        productsAndQty.Add(productName, orderDetails.Quantity);

                        total += orderDetails.Quantity * price;
                    }
                    orderForAdminVMs.Add(new OrderForAdminVM {

                        OrderNumber=order.OrderId,
                        Username=username,
                        Total=total,
                        ProductsAndQty=productsAndQty,
                        CreatedAt=order.CreatedAt


                    });

                }

            }



            return View(orderForAdminVMs);
        }
    }
}