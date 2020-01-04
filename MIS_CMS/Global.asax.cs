using MIS_CMS.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MIS_CMS
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_AuthenticateRequest()
        {
            if (User == null)
            {
                return;
            }

            string username = User.Identity.Name;

            string[] roles = null;
            using (DB  db= new DB())
            {
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName.Equals(username));
                roles = db.UserRoles.Where(x => x.UserId == dto.Id).Select(x => x.Role.Name).ToArray();

            }
            IIdentity user_identity = new GenericIdentity(username);
            IPrincipal newuserobj = new GenericPrincipal(user_identity, roles);
            Context.User = newuserobj;

            




        }
    }
}
