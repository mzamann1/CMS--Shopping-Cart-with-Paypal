using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MIS_CMS
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
               name: "Account",
               url: "Account/{action}/{id}",
               new { controller = "Account", action = "Index", id = UrlParameter.Optional }, new[] { "MIS_CMS.Controllers" }
               );




            routes.MapRoute(
               name: "Cart",
               url: "Cart/{action}/{id}",
               new { controller = "Cart", action = "Index", id = UrlParameter.Optional }, new[] { "MIS_CMS.Controllers" }
               );


            routes.MapRoute(
               name: "Shop",
               url: "Shop/{action}/{name}",
               new { controller = "Shop", action = "Index", name=UrlParameter.Optional }, new[] { "MIS_CMS.Controllers" }
               );


            routes.MapRoute(
                name: "SidebarPartial",
                url: "Pages/SidebarPartial",
                new { controller = "Pages", action = "SidebarPartial" }, new[] { "MIS_CMS.Controllers" }
                );

            routes.MapRoute(
                name: "PageMenuPartial",
                url:"Pages/PageMenuPartial",
                new { controller = "Pages", action = "PageMenuPartial" }, new[] { "MIS_CMS.Controllers" }
                );

            routes.MapRoute(
                name: "Page",
                "{page}",
                new { controller="Pages",action="Index" }, new[] { "MIS_CMS.Controllers" }
                );

            routes.MapRoute(
                name: "Default",
                url: "",
                new { controller = "Pages", action = "Index" }, new[] { "MIS_CMS.Controllers" }
                );

            
        }
    }
}
