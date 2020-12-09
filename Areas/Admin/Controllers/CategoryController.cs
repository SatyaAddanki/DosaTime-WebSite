using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewBook.DataAccess.Repository;
using NewBook.Models;

namespace NewBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class CategoryController : Controller
    {
        private readonly IRepository _ir;

        public CategoryController(IRepository ir)
        {
            _ir = ir;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Upsert(int? Id)
        {
            Category c1 =new Category();
            if(Id ==null)
            {
                return View(c1);
            }
           c1= _ir.Category.Get(Id.GetValueOrDefault());
            return View(c1);       
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Category c2)
        {
            Category c1 = new Category();
            c1 = _ir.Category.Get(c2.Id);
            if (c1 == null)
            {
                _ir.Category.Add(c2);
                return RedirectToAction(nameof(Index));
            }
            _ir.Category.Update(c2);
            return RedirectToAction(nameof(Index));
        }

        #region API Call
        [HttpGet]
        public IActionResult GetAll()
        {
            var iobj = _ir.Category.GetAll();
            return Json(new { data = iobj });
        }

        [HttpDelete]
        public IActionResult Delete(int Id)
        {
            var Cat=_ir.Category.Get(Id);
            if(Cat==null)
            {
                return Json(new { status=false,mesage="failed"});
            }
            _ir.Category.Remove(Cat.Id);
            return Json(new { status = true, mesage = "Success" });
        }
        #endregion

    }
}
