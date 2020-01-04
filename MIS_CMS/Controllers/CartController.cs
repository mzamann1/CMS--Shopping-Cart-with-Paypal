using MIS_CMS.Models.Data;
using MIS_CMS.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace MIS_CMS.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            if(cart.Count==0 || Session["cart"] == null)
            {
                ViewBag.Message = "Cart is Empty!!!!";
                return View();
            }

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
            CartVM model = new CartVM();

            int qty = 0;

            decimal price = 0;
            if (Session["cart"]!=null)
            {
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Price;

                }

                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                model.Quantity = 0;
                model.Price = 0;
            }

            return PartialView(model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            List <CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            CartVM model = new CartVM();

            using (DB db= new DB())
            {
                ProductDTO product = db.Products.Find(id);

                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image=product.ImageName

                    });
                    
                }
                else
                {
                    productInCart.Quantity++;
                }
            }

            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            Session["cart"] = cart;


            return PartialView(model);


        }

        public JsonResult IncrementProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (DB db = new DB())
            {
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
                model.Quantity++;

                var result = new { qty = model.Quantity, price = model.Price };

                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        public JsonResult DecrementProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (DB db = new DB())
            {
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                if (model.Quantity > 1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                var result = new { qty = model.Quantity, price = model.Price };

                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        public void RemoveProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (DB db= new DB())
            {
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                cart.Remove(model);



            }

        }

        public ActionResult PaypalPartial()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            return PartialView(cart);
        }
        // POST: /Cart/PlaceOrder
        [HttpPost]
        public void PlaceOrders()
        {
            // Get cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // Get username
            string username = User.Identity.Name;

            int orderId = 0;

            using (DB db = new DB())
            {
                // Init OrderDTO
                OrderDTO orderDTO = new OrderDTO();

                // Get user id
                var q = db.Users.FirstOrDefault(x => x.UserName == username);
                int userId = q.Id;

                // Add to OrderDTO and save
                orderDTO.UserId = userId;
                orderDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDTO);

                db.SaveChanges();

                // Get inserted id
                orderId = orderDTO.OrderId;

                // Init OrderDetailsDTO
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                // Add to OrderDetailsDTO
                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);

                    db.SaveChanges();
                }
            }

            // Email admin
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("20dc3a8ad120f3", "0d8a686d95066d"),
                EnableSsl = true
            };
            client.Send("admin@example.com", "admin@example.com", "New Order", "You have a new order. Order number " + orderId);

            // Reset session
            Session["cart"] = null;
        }
    }
}