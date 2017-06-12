using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using YakShop.Models;
using System.Web.Script.Serialization;
using System.Xml;

namespace YakShop.Controllers
{
    public class yakshopController : Controller
    {
        [HttpGet]
        public JsonResult stock(int T)
        {
            return Json(new clsWebShop().getstock(T), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult herd(int T)
        {
            return Json(new clsWebShop().getherd(T), JsonRequestBehavior.AllowGet);
        }
        public ActionResult order()
        {
            return View();
        }
        [HttpPost]
        public ActionResult orderonline(int T, string customer, double milk, int skins)
        {
            onlineOrder customerorder = new onlineOrder();
            customerorder.customer = customer;
            customerorder.order = new Models.stock()
            {
                milk = milk,
                skins = skins
            };
            JsonResult jorder = order(T, customerorder);
            JavaScriptSerializer objdez = new JavaScriptSerializer();
            if ((jorder.Data.ToString() == "Not Found"))
            {
                ViewBag.Message = "Stock Not Available for required Milk and Skins";
            }
            else
            {
                stock orderinfo = objdez.Deserialize<stock>(jorder.Data.ToString());
                ViewBag.Message = orderinfo;
            }
            return View("order");
        }
        public JsonResult orderReport()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Server.MapPath("~/Database/orders.xml"));
            var lstorders = (from XmlNode n in doc.SelectNodes("//order")
                             select new
                             {
                                 customer = n.ChildNodes[1].InnerText,
                                 T = n.ChildNodes[0].InnerText,
                                 milk = Convert.ToDouble(n.ChildNodes[2].InnerText),
                                 skins = Convert.ToInt32(n.ChildNodes[3].InnerText)
                             }).ToList();
            var orderSummery = from o in lstorders
                               group o by o.T into gorder
                               select new {
                                   T = gorder.Key,
                                   Milksum = gorder.Sum(g => g.milk),
                                   Skinssum = gorder.Sum(g => g.skins),
                                   customers = gorder.Count()

            };
            return Json(orderSummery, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult order(int T, onlineOrder customerorder)
        {
            stock objStock = (stock)new clsWebShop().getstock(T);
            List<onlineOrder> lstoders = new List<onlineOrder>();
            double totalmilkordertilldate = 0.0;
            int totalskinordertilldate = 0;

            lstoders = new clsWebShop().getOrderHistory();
            if (lstoders != null && lstoders.Count() > 1)
            {
                totalmilkordertilldate = lstoders.Sum(m => m.order.milk);
                totalskinordertilldate = lstoders.Sum(s => s.order.skins);
            }
            if ((objStock.milk - totalmilkordertilldate) >= customerorder.order.milk && customerorder.order.skins <= (objStock.skins - totalskinordertilldate))
            {
                Response.StatusCode = 201;
                new clsWebShop().generateorderxml(customerorder.customer, customerorder.order.milk, customerorder.order.skins, T);
                return Json("{ \"milk\" :" + customerorder.order.milk.ToString() + ", \"skins\" : " + customerorder.order.skins.ToString() + "}", JsonRequestBehavior.AllowGet);
            }
            else if ((objStock.milk - totalmilkordertilldate) >= customerorder.order.milk && customerorder.order.skins > (objStock.skins - totalskinordertilldate))
            {
                Response.StatusCode = 206;
                new clsWebShop().generateorderxml(customerorder.customer, customerorder.order.milk, 0, T);
                return Json("{ \"milk\" :" + customerorder.order.milk.ToString() + "}", JsonRequestBehavior.AllowGet);
            }
            else if ((objStock.milk - totalmilkordertilldate) < customerorder.order.milk && customerorder.order.skins <= (objStock.skins - totalskinordertilldate))
            {
                Response.StatusCode = 206;
                new clsWebShop().generateorderxml(customerorder.customer, 0, customerorder.order.skins, T);
                return Json("{ \"skins\" :" + customerorder.order.skins.ToString() + "}", JsonRequestBehavior.AllowGet);
            }
            else
            {
                Response.StatusCode = 404;
                return Json("Not Found", JsonRequestBehavior.AllowGet);
            }
        }
    }

}