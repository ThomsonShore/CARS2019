using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using CARS2019.Models;
using System.Security;
using System.Configuration;
using Galactic.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Security.Principal;

namespace CARS2019.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            string serverName = ConfigurationManager.AppSettings["ADServer"];
            if (ModelState.IsValid)
            {
                SecureString securePwd = null;
                if (model.Password != null)
                {
                    securePwd = new SecureString();
                    foreach (char chr in model.Password.ToCharArray())
                    {
                        securePwd.AppendChar(chr);
                    }
                }
                try
                {
                    //Check user credentials
                    ActiveDirectory adVerifyUser = new ActiveDirectory(serverName, model.UserName, securePwd);

                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    List<SearchResultEntry> results = adVerifyUser.GetEntriesBySAMAccountName(model.UserName);
                    UserProfile usrLoginProfile = new UserProfile();
                    if (results.Count > 0)
                    {
                        User adusr = new User(adVerifyUser, results[0]);
                        usrLoginProfile.UserName = adusr.DisplayName;
                        //usrLoginProfile.Groups = adusr.Groups;
                        Session["adFullUserName"] = usrLoginProfile.UserName;
                        HttpCookie carsCookie = new HttpCookie("carsCookie");
                        carsCookie.Value = usrLoginProfile.UserName;
                        carsCookie.Expires = DateTime.Now.AddHours(8);
                        Response.SetCookie(carsCookie);
                        //Response.Flush();
                    }
                    UserPermissions userPermissions = new UserPermissions();

                if (CheckUserInGroup("APP_CARS_Admin" , model.UserName))
                    {
                        Session["canDeleteEntry"] = userPermissions.canDeleteEntry = true;
                        Session["canSeeCorrectiveAction"] = userPermissions.canSeeCorrectiveAction = true;
                        Session["canSeeEmployee"] = userPermissions.canSeeEmployee = true;
                        Session["canEditEntry"] = userPermissions.canEditEntry = true;
                    }
                    else
                    {
                        Session["canDeleteEntry"] = userPermissions.canDeleteEntry = false;
                        Session["canSeeCorrectiveAction"] = userPermissions.canSeeCorrectiveAction = false;
                        Session["canSeeEmployee"] = userPermissions.canSeeEmployee = false;
                        Session["canEditEntry"] = userPermissions.canEditEntry = true;
                    }

                    return RedirectToLocal(returnUrl);
                }
                catch
                {
                    // If we got this far, something failed, redisplay form
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                    var emailBody = "CARS Login Failure for user: " + model.UserName;
                    MailSendHelper.SendingErrorEmail("donotreply@tshore.com", "jbrennan@tshore.com", emailBody);
                }
            }

            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [SessionExpire]
        public ActionResult UserProfile()
        {
            string serverName = ConfigurationManager.AppSettings["ADServer"];
            string userName = ConfigurationManager.AppSettings["ADUserName"];
            string password = ConfigurationManager.AppSettings["ADPassword"];

            if (System.Web.HttpContext.Current.User.Identity.Name != "jbrennan")
            {
                return RedirectToAction("Index", "Home");
            }
            SecureString securePwd = null;
            if (password != null)
            {
                securePwd = new SecureString();
                foreach (char chr in password.ToCharArray())
                {
                    securePwd.AppendChar(chr);
                }
            }
            UserProfile usrProfile = new UserProfile();
            try
            {
                ActiveDirectory adConnect = new ActiveDirectory(serverName, userName, securePwd);
                List<SearchResultEntry> results = adConnect.GetEntriesBySAMAccountName(System.Web.HttpContext.Current.User.Identity.Name);
                if (results.Count > 0)
                {
                    User usr = new User(adConnect, results[0]);
                    usrProfile.FirstName = usr.FirstName;
                    usrProfile.LastName = usr.LastName;
                    usrProfile.Manager = usr.Manager;
                    usrProfile.Department = usr.Department;
                    usrProfile.Division = usr.Division;
                    usrProfile.EmployeeId = usr.EmployeeId;
                    usrProfile.EmployeeNumber = usr.EmployeeNumber;
                    usrProfile.PhoneNumber = usr.PhoneNumber;
                    usrProfile.StreetAddress = usr.StreetAddress;
                    usrProfile.Title = usr.Title;
                    usrProfile.UserName = usr.DisplayName;
                    usrProfile.Groups = usr.Groups;
                }
            }
            catch
            {
                // unable to connect AD
                ModelState.AddModelError("", "Unable to connect AD!");
                var emailBody = "CARS AD Failure for user: " + System.Web.HttpContext.Current.User.Identity.Name;
                MailSendHelper.SendingErrorEmail("donotreply@tshore.com", "jbrennan@tshore.com", emailBody);
            }
            return View(usrProfile);
        }

        // Remove this from production, can be used to elevate privlege on an authorized account.****************************************************************
        //public ActionResult toggleDeletePermission()
        //{
        //    bool toggle = Convert.ToBoolean((Session["canDeleteEntry"] ?? "False"));
        //    toggle = !toggle;
        //    Session["canDeleteEntry"] = toggle.ToString();
        //    return RedirectToAction("UserProfile", "Account");
        //}

        //public ActionResult toggleCanSeeCorrectiveAction()
        //{
        //    bool toggle = Convert.ToBoolean((Session["canSeeCorrectiveAction"] ?? "False"));
        //    toggle = !toggle;
        //    Session["canSeeCorrectiveAction"] = toggle.ToString();
        //    return RedirectToAction("UserProfile", "Account");
        //}

        //public ActionResult toggleCanEditEntry()
        //{
        //    bool toggle = Convert.ToBoolean((Session["canEditEntry"] ?? "False"));
        //    toggle = !toggle;
        //    Session["canEditEntry"] = toggle.ToString();
        //    return RedirectToAction("UserProfile", "Account");
        //}

        //public ActionResult toggleCanSeeEmployee()
        //{
        //    bool toggle = Convert.ToBoolean((Session["canSeeEmployee"] ?? "False"));
        //    toggle = !toggle;
        //    Session["canSeeEmployee"] = toggle.ToString();
        //    return RedirectToAction("UserProfile", "Account");
        //}

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }





        private bool CheckUserInGroup(string group, string username)
        {
            string serverName = ConfigurationManager.AppSettings["ADServer"];
            string userName = ConfigurationManager.AppSettings["ADUserName"];
            string password = ConfigurationManager.AppSettings["ADPassword"];
            bool result = false;
            SecureString securePwd = null;
            if (password != null)
            {
                securePwd = new SecureString();
                foreach (char chr in password.ToCharArray())
                {
                    securePwd.AppendChar(chr);
                }
            }
            try
            {
                ActiveDirectory adConnectGroup = new ActiveDirectory(serverName, userName, securePwd);
                SearchResultEntry groupResult = adConnectGroup.GetEntryByCommonName(group);
                Group grp = new Group(adConnectGroup, groupResult);
                SecurityPrincipal userPrincipal = grp.Members.Find(sp => sp.SAMAccountName.ToLower() == username.ToLower());
                if (userPrincipal != null)
                {
                    result = true;
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }
        #endregion
    }
}