using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewBook.DataAccess.Data;
using NewBook.DataAccess.Repository;
using NewBook.Models;

namespace NewBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _ir;

        public UserController(ApplicationDbContext ir)
        {
            _ir = ir;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }



        #region API Call
        [HttpGet]
        public IActionResult GetAll()
        {
            var UserList = _ir.ApplicationUsers.ToList();
            foreach (var Usr in UserList)
            {
                var roleId = _ir.UserRoles.FirstOrDefault(i => i.UserId == Usr.Id);
                Usr.Role = _ir.Roles.FirstOrDefault(i => i.Id == roleId.RoleId).Name;
            }
            return Json(new { data = UserList });
        }

        //[HttpDelete]
        //public IActionResult Delete(int Id)
        //{
        //    var Cat=_ir.User.Get(Id);
        //    if(Cat==null)
        //    {
        //        return Json(new { status=false,mesage="failed"});
        //    }
        //    _ir.User.Remove(Cat.Id);
        //    return Json(new { status = true, mesage = "Success" });
        //}
        #endregion


    }
}
