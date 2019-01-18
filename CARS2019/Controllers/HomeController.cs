using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using CARS2019.Models;

namespace CARS2019.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Session["adFullUserName"] == null)
            {
                FormsAuthentication.SignOut();
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "CARS 2019";

            ControllerBase checkGroup = new ControllerBase();
            //Pub_Services
            bool GroupCheck = checkGroup.CheckUserInGroup("IT_DBA"); // hard coded check for an AD group here will probably want to move these into a db 12-21-18 jb
            //bool DBACheck = checkGroup.CheckUserInGroup("Pub_Services");
            if (GroupCheck)
            {
                ViewBag.Message = "Woo, you are an IT Database Administrator!!! Lucky U ;)";
                //ViewBag.Message = "Woo, you are in Pub_Services!!! Lucky U ;)";
                ViewBag.Groups = checkGroup.GetGroups(System.Web.HttpContext.Current.User.Identity.Name.ToString());
            }
            else
            {
                ViewBag.Message = "CARS 2019";
                ViewBag.Groups = checkGroup.GetGroups(System.Web.HttpContext.Current.User.Identity.Name.ToString());
            }

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Please contact for support:";

            return View();
        }



    }
}