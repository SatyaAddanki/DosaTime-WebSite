using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NewBook.DataAccess.Data;
using NewBook.Models;
using Stripe;

namespace NewBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrderDetailsController : Controller
    {
        private ApplicationDbContext _db;
        public OrderDetailsController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
           var claims= (ClaimsIdentity)User.Identity;
           var name = claims.FindFirst(ClaimTypes.NameIdentifier);
         IEnumerable<OrderHeader> om= _db.OrderHeader.ToList().Where(i => i.ApplicationUserId == name.Value);
            return View(om);
        }

        public IActionResult GetOrderDetails(string status)
        {
            IEnumerable<OrderHeader> om = _db.OrderHeader.ToList().Where(i => i.PaymentStatus == status);
            return View("Index",om);
        }


        public IActionResult CancelOrder(int Id)
        {
           OrderHeader oh= _db.OrderHeader.FirstOrDefault(i => i.Id == Id);
            // refunding

           var service= new RefundService();
            var options = new RefundCreateOptions()
            {
                Amount = Convert.ToInt32(oh.OrderTotal),
                Reason=RefundReasons.RequestedByCustomer

            };
          Refund refund=  service.Create(options);
            if(refund.Status=="succeeded")
            {

            }
            return View(oh);
        }
    }
}
