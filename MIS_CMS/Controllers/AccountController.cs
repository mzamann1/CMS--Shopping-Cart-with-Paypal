using MIS_CMS.Models.Data;
using MIS_CMS.Models.ViewModels.Account;
using MIS_CMS.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MIS_CMS.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return RedirectToAction("login");
        }
        [ActionName("login")]
        [HttpGet]
        public ActionResult Login()
        {
            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
            {
                return RedirectToAction("user-profile");

            }

            return View();
        }

        [HttpPost]
        [ActionName("login")]
        public ActionResult Login(LoginUserVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            bool isValid = false;
            using (DB db=new DB())
            {
                if (db.Users.Any(x=>x.UserName.Equals(model.UserName)&&x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }

            if (!isValid)
            {
                ModelState.AddModelError("", "Invalid User Name or Password");
                return View(model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.UserName, model.RememberMe));
            }
            return View();
        }
        [HttpGet]
        [ActionName("logout")]
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("login");
        }

        [HttpGet]
        [ActionName("create-account")]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        [HttpPost]
        [ActionName("create-account")]
        public ActionResult CreateAccount(UserVM model)
        {
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }

            if(model.Password!=model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Password Doesnot Matches!!");
                return View("CreateAccount", model);

            }

            using (DB db = new DB())
            {
                if (db.Users.Any(x=>x.UserName.Equals(model.UserName)))
                {
                    ModelState.AddModelError("", "UserName :"+model.UserName+" Already Exists!");
                    model.UserName = "";
                    return View("CreateAccount", model);

                }

                if (db.Users.Any(x => x.EmailAddress.Equals(model.EmailAddress)))
                {
                    ModelState.AddModelError("", "Email :" + model.EmailAddress + " Already Exists!");
                    model.EmailAddress = "";
                    return View("CreateAccount", model);

                }


                UserDTO dto = new UserDTO()
                {
                    FirstName=model.FirstName,
                    LastName=model.LastName,
                    EmailAddress=model.EmailAddress,
                    UserName=model.UserName,
                    Password=model.Password
                };
                db.Users.Add(dto);
                db.SaveChanges();

                int id = dto.Id;
                UserRoleDTO userRoleDTO = new UserRoleDTO() {
                    UserId=id,
                    RoleId=2

                };

                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();

            }
            TempData["SM"] = "Account Created Successfully!!";
            return RedirectToAction("login");
        }

        [HttpGet]
        [Authorize]
        public ActionResult UserNavPartial()
        {
            string username = User.Identity.Name.ToString();

            UserNavPartialVM model;

            using (DB db= new DB())
            {
              UserDTO dto=  db.Users.FirstOrDefault(x => x.UserName == username);

                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName=dto.LastName
                };
            }
            return PartialView(model);
        }
        [HttpGet]
        //[ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            string username = User.Identity.Name;
            UserProfileVM model;
            using (DB db= new DB())
            {
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == username);

                model = new UserProfileVM(dto);

            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        // [ActionName("user-profile")]
        public ActionResult UserProfile(UserProfileVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (!string.IsNullOrWhiteSpace(model.Password)) {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("","Password Doen't Matches");
                    TempData["SM"]= "Password Don't Matches";
                    return View(model);
                }



            }


            using (DB db= new DB())
            {
                string username = User.Identity.Name;

                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.UserName == username))
                {
                    ModelState.AddModelError("", "Username :   "+username+"  already exists!");
                    model.UserName = "";
                    TempData["SM"] = "Username :   " + username + "  already exists!";
                    return View(model);

                }

                UserDTO dto = db.Users.Find(model.Id);
                
                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.UserName = model.UserName;
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = dto.Password;
                }

                db.SaveChanges();
                TempData["SM"] = "Success!!";
                return Redirect("~/account/UserProfile");

            }

            

            
           
        }

        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            List<OrderForUserVM> ordersUser = new List<OrderForUserVM>();


            using (DB db= new DB())
            {
                UserDTO user = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();

                int userid = user.Id;

                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userid).ToArray().Select(x => new OrderVM(x)).ToList();

                foreach (var order in orders)
                {
                    Dictionary<string, int> productAndQty = new Dictionary<string, int>();

                    decimal total = 0m;

                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();


                    foreach (var orderDetails in orderDetailsDTO)
                    {
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        decimal price = product.Price;

                        string productName = product.Name;

                        productAndQty.Add(productName, orderDetails.Quantity);

                        total += orderDetails.Quantity * price;





                    }

                    ordersUser.Add(new OrderForUserVM()
                    {
                        OrderNumber=order.OrderId,
                        Total=total,
                        ProductsAndQty=productAndQty,
                        CreatedAt=order.CreatedAt

                    });

                }
            }

            return View(ordersUser);
        }

        
    }
}