using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewBook.DataAccess.Data;
using NewBook.Models;

namespace NewBook.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _db;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }

       // public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            public string Name { get; set; }
            public string Address { get; set; }
            public string PostalCode { get; set; }

            public string Username { get; set; }




        }

        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var usr = _db.ApplicationUsers.FirstOrDefault(i => i.UserName == userName);
            //Username = userName;

            Input = new InputModel
            {
                Name=usr.Name,
                Address=usr.Address,
                PostalCode=usr.PostalCode,
                PhoneNumber = phoneNumber,
                Username=usr.UserName
                
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var usern = await _userManager.GetUserAsync(User);
            var usr = _db.ApplicationUsers.FirstOrDefault(i => i.UserName == usern.UserName);
            var user = new ApplicationUser()
            {
                Name = usr.Name,
                PhoneNumber = usr.PhoneNumber,
                Address = usr.Address,
                PostalCode = usr.PostalCode,
                UserName=usr.UserName
            };
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var usern = await _userManager.GetUserAsync(User);
            var user = new ApplicationUser()
            {
                Name = Input.Name,
                PhoneNumber = Input.PhoneNumber,
                Address = Input.Address,
                PostalCode = Input.PostalCode
            };

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            //var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            //if (Input.PhoneNumber != phoneNumber)
            //{
            //    var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
            //    if (!setPhoneResult.Succeeded)
            //    {
            //        StatusMessage = "Unexpected error when trying to set phone number.";
            //        return RedirectToPage();
            //    }
            //}
            var usr = _db.ApplicationUsers.FirstOrDefault(i => i.UserName == Input.Username);
            usr.PostalCode = user.PostalCode;
            usr.PhoneNumber = user.PhoneNumber;
            usr.Address = user.Address;
            usr.Name = user.Name;
            _db.SaveChanges();
            await _signInManager.RefreshSignInAsync(usern);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
