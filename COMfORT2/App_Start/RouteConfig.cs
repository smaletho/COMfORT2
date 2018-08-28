using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace COMfORT2
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{action}.ashx");
            

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Default", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                "COMfORT2.ImageManager",
                "{action}.ashx"
            );
            //routes.Add(new Route
            //(
            //     "{action}.ashx"
            //     , new ImageRouteHandler()
            //));
            //routes.MapRoute("Images", "ShowImage.ashx");
            //RouteTable.Routes.Add(new Route("files/upload", new HttpHandlerRoute("~/ShowImage.ashx")));
        }
    }
}
