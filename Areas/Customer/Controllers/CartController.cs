using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewBook.DataAccess.Data;
using NewBook.DataAccess.Repository;
using NewBook.Models;
using NewBook.Models.ViewModels;
using Stripe;

namespace NewBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IRepository _ir;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _user;
        [BindProperty]
        public ShoppinCartVM sm { get; set; }
        public CartController(IRepository ir, UserManager<IdentityUser> user, ApplicationDbContext db)
        {
            _ir = ir;
            _user = user;
            _db = db;
        }
        public IActionResult Index()
        {
            var claims = (ClaimsIdentity)User.Identity;
            var name = claims.FindFirst(ClaimTypes.NameIdentifier);
            sm = new ShoppinCartVM()
            {
                CartList = _db.ShoppingCarts.Include("Product").ToList().Where(i => i.ApplicationUserId == name.Value).ToList(),
                Orderheader = new Models.OrderHeader()
            };
            sm.Orderheader.OrderTotal = 0;
            foreach (var sp in sm.CartList)
            {
                sp.Price = sp.Product.Price;
                sm.Orderheader.OrderTotal += sp.Price * sp.Count;
            }
            return View(sm);
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _ir.Shopping.Get(cartId);
            if (cart.Count == 1)
            {
                _ir.Shopping.Delete(cart.Id);
                var cnt = _ir.Shopping.GetAll().ToList().Where(i => i.ApplicationUserId == cart.ApplicationUserId);
                HttpContext.Session.SetInt32("ShoppingCart", cnt.Count() - 1);
            }
            else
            {
                cart.Count -= 1;

            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int cartId)
        {
            var cart = _ir.Shopping.Get(cartId);

            _ir.Shopping.Delete(cart.Id);
            var cnt = _ir.Shopping.GetAll().ToList().Where(i => i.ApplicationUserId == cart.ApplicationUserId);
            HttpContext.Session.SetInt32("ShoppingCart", cnt.Count() - 1);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Plus(int cartId)
        {
            var cart = _ir.Shopping.Get(cartId);
            cart.Count += 1;
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult Summary()
        {
            var claims = (ClaimsIdentity)User.Identity;
            var name = claims.FindFirst(ClaimTypes.NameIdentifier);
            sm = new ShoppinCartVM()
            {
                CartList = _db.ShoppingCarts.Include("Product").ToList().Where(i => i.ApplicationUserId == name.Value),
                Orderheader = new Models.OrderHeader()
            };

            foreach (var li in sm.CartList)
            {
                sm.Orderheader.OrderTotal += li.Product.Price * li.Count;
            }
            sm.Orderheader.ApplicationUser = _db.ApplicationUsers.FirstOrDefault(i => i.Id == name.Value);
            return View(sm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public IActionResult SummaryPost(string StripeToken)
        {
            if (ModelState.IsValid)
            {
                var claims = (ClaimsIdentity)User.Identity;
                var name = claims.FindFirst(ClaimTypes.NameIdentifier);
                sm.CartList = _db.ShoppingCarts.Include("Product").ToList().Where(i => i.ApplicationUserId == name.Value).ToList();
                sm.Orderheader.ApplicationUser = _db.ApplicationUsers.FirstOrDefault(m => m.Id == name.Value);
                sm.Orderheader.ApplicationUserId = name.Value;
                sm.Orderheader.OrderDate = DateTime.Now;
                sm.Orderheader.PaymentStatus = "Pending";
                _db.OrderHeader.Add(sm.Orderheader);
                _db.SaveChanges();
                foreach (var cart in sm.CartList)
                {
                    OrderDetail od = new OrderDetail()
                    {
                        OrderId = sm.Orderheader.Id,
                        Price = cart.Product.Price * cart.Count,
                        ProductId = cart.ProductId,
                        Count = cart.Count
                    };
                    sm.Orderheader.OrderTotal += cart.Count * cart.Product.Price;
                    _db.OrderDetails.Add(od);
                }
                _db.ShoppingCarts.RemoveRange(sm.CartList);
                _db.SaveChanges();
                HttpContext.Session.SetInt32("ShoppingCart", 0);

                if (StripeToken == null)
                {
                    ModelState.AddModelError("key","Transaction failed");
                }
                else
                {
                    var options = new ChargeCreateOptions()
                    {
                        Amount = Convert.ToInt32(sm.Orderheader.OrderTotal),
                        Description = "OrderId" + sm.Orderheader.Id,
                        Currency = "usd",
                        Source = StripeToken
                    };

                    var service = new ChargeService();
                    Charge charge = service.Create(options);

                    if (charge.BalanceTransactionId != null)
                    {
                        sm.Orderheader.PaymentStatus = "Success";
                    }
                    else
                    {
                        sm.Orderheader.PaymentStatus = "Rejected";
                    }
                    if (charge.Status.ToLower() == "succeeded")
                    {
                        sm.Orderheader.PaymentStatus = "Success Transaction";
                    }
                    _db.SaveChanges();
                }
                return RedirectToAction(nameof(Confirmation), new { id = @sm.Orderheader.Id });
            }
            return RedirectToAction(nameof(Summary));
         

        }
        [HttpGet]
        public IActionResult Confirmation(int Id)
        {
            return View(Id);
        }
    }
}
