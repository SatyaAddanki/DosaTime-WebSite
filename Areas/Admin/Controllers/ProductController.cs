using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NewBook.DataAccess.Repository;
using NewBook.Models;
using NewBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace NewBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IRepository _ir;
        private readonly IWebHostEnvironment _hostenvr;

        public ProductController(IRepository ir, IWebHostEnvironment hostenvr)
        {
            _ir = ir;
            _hostenvr = hostenvr;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            ProductViewModel productViewModel = new ProductViewModel()
            {
                product = new Product(),
                categoryList = _ir.Category.GetAll().Select(i => new SelectListItem() { Text = i.Name, Value = i.Id.ToString() })

            };
            if (id == null)
            {
                return View(productViewModel);
            }
            else
            {
                productViewModel.product = _ir.Product.Get(id.GetValueOrDefault());
                if (productViewModel.product == null)
                {
                    return NotFound();
                }
                return View(productViewModel);
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductViewModel productViewModel)
        {
            
                var pr1 = _ir.Product.Get(productViewModel.product.Id);
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    string filename = Guid.NewGuid().ToString();
                    string root = _hostenvr.WebRootPath;
                    var ext = Path.GetExtension(files[0].FileName);
                    var total = Path.Combine(root, @"Products", filename + ext);
                    if(productViewModel.product.ImageUrl!=null)
                    {
                        var img=Path.Combine(root, productViewModel.product.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(img))
                        {
                            System.IO.File.Delete(img);
                        }
                    }
                    using (var fs = new FileStream(total, FileMode.Create))
                    {
                        files[0].CopyTo(fs);
                    }
                    productViewModel.product.ImageUrl = @"\Products\" + filename + ext;
                }

                if (pr1 == null)
                {
                    _ir.Product.Add(productViewModel.product);
                }
                else
                {
                    _ir.Product.Update(productViewModel.product);

                }
            
            return RedirectToAction(nameof(Upsert));
        }

        public IActionResult Delete()
        {
            return View();
        }



    }
}
