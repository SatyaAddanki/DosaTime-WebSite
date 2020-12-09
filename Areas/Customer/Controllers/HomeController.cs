using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NewBook.DataAccess.Repository;
using NewBook.Models;
using NewBook.Models.ViewModels;

namespace NewBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRepository _ir;

        public HomeController(ILogger<HomeController> logger, IRepository ir)
        {
            _logger = logger;
            _ir = ir;
        }

        public IActionResult Index()
        {
           IEnumerable<Product> product=_ir.Product.GetAll();
            var claims = (ClaimsIdentity)User.Identity;
            var name = claims.FindFirst(ClaimTypes.NameIdentifier);
            if(name!=null)
            {
                //var count = _ir.Shopping.GetAll().Select(s => s.ApplicationUserId == name.Value).ToList().Count();
                var count = _ir.Shopping.GetAll().ToList().Where(s => s.ApplicationUserId == name.Value).Count();
                HttpContext.Session.SetInt32("ShoppingCart", count);
            }
            return View(product);
        }

        [HttpGet]
        public IActionResult Details(int Id)
        {
            Product pr =_ir.Product.Get(Id);
            ShoppingCart shoppingCart = new ShoppingCart()
            {
                Product = pr,
                ProductId = pr.Id
            };
            return View(shoppingCart);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart sp)
        {
            sp.Id = 0;
            if(ModelState.IsValid)
            {
                var claims=(ClaimsIdentity)User.Identity;
                var name= claims.FindFirst(ClaimTypes.NameIdentifier);
                sp.ApplicationUserId = name.Value;
                ShoppingCart sdb=_ir.Shopping.ExistingProducts(sp);
                if(sdb==null)
                {
                    _ir.Shopping.Add(sp);
                }
                else
                {
                    sp.Count += sdb.Count;
                    _ir.Shopping.Update(sp);
                }

                var count = _ir.Shopping.GetAll().ToList().Where(s => s.ApplicationUserId == name.Value).Count();
                HttpContext.Session.SetInt32("ShoppingCart", count);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                Product pr = _ir.Product.Get(sp.ProductId);
                ShoppingCart shoppingCart = new ShoppingCart()
                {
                    Product = pr,
                    ProductId = pr.Id
                };
                return View(shoppingCart);
            }
            
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
