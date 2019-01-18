using Galactic.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.Mvc;
using CARS2019.Models;
using System.Text;

namespace CARS2019.Controllers
{
    public class ControllerBase : Controller
    {
        public bool CheckUserInGroup(string group)
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
                SecurityPrincipal userPrincipal = grp.Members.Find(sp => sp.SAMAccountName.ToLower() == System.Web.HttpContext.Current.User.Identity.Name.ToLower());
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

        public string GetGroups(string UserName)
        {
            string result = "";
            if (UserName != "")
            {
                string serverName = ConfigurationManager.AppSettings["ADServer"];
                string userName = ConfigurationManager.AppSettings["ADUserName"];
                string password = ConfigurationManager.AppSettings["ADPassword"];
                
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
                    UserProfile usrProfile = new UserProfile();
                    List<SearchResultEntry> results = adConnectGroup.GetEntriesBySAMAccountName(UserName);

                    if (results.Count > 0)
                    {
                        User usr = new User(adConnectGroup, results[0]);
                        usrProfile.Groups = usr.Groups;

                        StringBuilder sb = new StringBuilder();
                        foreach (string group in usrProfile.Groups)
                        {
                            sb.Append(group.ToString());
                        }
                        string strGroups = sb.ToString();
                        result = strGroups;
                    }

                }
                catch
                {
                    result = "An error occured retreiving group membership.";
                }
                return result;
            }
            else
            {
                result = "Log in to see Group Memberships";
                return result;
                    
            }

        }


    }
}