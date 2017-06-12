using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace YakShop
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
              name: "stock",
              url: "yak-shop/stock/{T}",
                defaults: new { controller = "yakshop", action = "stock" }
             );
            routes.MapRoute(
              name: "herd",
              url: "yak-shop/herd/{T}",
                defaults: new { controller = "yakshop", action = "herd" }
             );
            routes.MapRoute(
              name: "orderReport",
              url: "yak-shop/orderReport",
              defaults: new { controller = "yakshop", action = "orderReport" }
             );
            routes.MapRoute(
              name: "order",
              url: "yak-shop/order/{T}",
                defaults: new { controller = "yakshop", action = "order" }
             );
            routes.MapRoute(
              name: "onlineorder_form",
              url: "yak-shop/order",
                defaults: new { controller = "yakshop", action = "order" }
             );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
