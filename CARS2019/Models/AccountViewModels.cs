using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CARS2019.Models
{
    public class UserProfile

    {
        public string UserName { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public string DisplayName { get; set; }
        public string Division { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Manager { get; set; }
        public string PhoneNumber { get; set; }
        public string StreetAddress { get; set; }
        public List<string> Groups { get; set; }
    }

    public class UserPermissions
    {
        public bool canSeeEmployee { get; set; } = false;
        public bool canSeeCorrectiveAction { get; set; } = false;
        public bool canDeleteEntry { get; set; } = false;
        public bool canEditEntry { get; set; } = false;

    }

    public class SessionExpireAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContext ctx = HttpContext.Current;
            // check  sessions here
            if (HttpContext.Current.Session["adFullUserName"] == null)
            {
                filterContext.Result = new RedirectResult("~/Home/Index");
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }


    public class LoginViewModel
    {

        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
