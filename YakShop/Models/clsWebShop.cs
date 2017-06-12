using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Configuration;
namespace YakShop.Models
{
    public class clsWebShop
    {
        private int yakageStandarddays = Convert.ToInt32(ConfigurationManager.AppSettings["yakageStandarddays"].ToString());
        private int yakageyears = Convert.ToInt32(ConfigurationManager.AppSettings["yakageyears"].ToString());
        public List<onlineOrder> getOrderHistory()
        {
            List<onlineOrder> lstorders = null;
            if (File.Exists(HttpContext.Current.Server.MapPath("~/Database/orders.xml")))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(HttpContext.Current.Server.MapPath("~/Database/orders.xml"));
                lstorders = (from XmlNode n in doc.SelectNodes("//order")
                             select new onlineOrder
                             {
                                 customer = n.ChildNodes[1].InnerText,
                                 order = new stock()
                                 {
                                     milk = Convert.ToDouble(n.ChildNodes[2].InnerText),
                                     skins = Convert.ToInt32(n.ChildNodes[3].InnerText)
                                 }
                             }).ToList<onlineOrder>();
            }
            return lstorders;
        }
        public bool generateorderxml(string customer, double milk, int skins, int T)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode ordersNode = null;
            if (File.Exists(HttpContext.Current.Server.MapPath("~/Database/orders.xml")))
            {
                doc.Load(HttpContext.Current.Server.MapPath("~/Database/orders.xml"));
                ordersNode = doc.SelectSingleNode("//orders");
            }
            else
            {
                XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(docNode);

                ordersNode = doc.CreateElement("orders");
                doc.AppendChild(ordersNode);
            }
            XmlNode orderNode = doc.CreateElement("order");
            ordersNode.AppendChild(orderNode);

            XmlNode TNode = doc.CreateElement("T");
            TNode.InnerText = T.ToString();
            orderNode.AppendChild(TNode);

            XmlNode customerNode = doc.CreateElement("customer");
            customerNode.InnerText = customer;
            orderNode.AppendChild(customerNode);

            XmlNode milkNode = doc.CreateElement("milk");
            milkNode.InnerText = milk.ToString();
            orderNode.AppendChild(milkNode);

            XmlNode skinNode = doc.CreateElement("skins");
            skinNode.InnerText = skins.ToString();
            orderNode.AppendChild(skinNode);

            doc.Save(HttpContext.Current.Server.MapPath("~/Database/orders.xml"));
            return true;
        }
        public object getstock(int T)
        {
            XElement xelement = null;
            IEnumerable<XElement> YakShopStock = null;
            stock objStock = null;
            try
            {
                if (File.Exists(HttpContext.Current.Server.MapPath("~/Database") + "/herd.xml"))
                {
                    xelement = XElement.Load(HttpContext.Current.Server.MapPath("~/Database") + "/herd.xml");
                    YakShopStock = xelement.Elements();
                    objStock = new stock();
                    objStock.milk = objStock.getMilkStock(YakShopStock.Count(), T, YakShopStock, yakageStandarddays, yakageyears);
                    objStock.skins = objStock.getskinsStock(YakShopStock.Count(), T, YakShopStock, yakageStandarddays, yakageyears);

                }
                return objStock;
            }
            catch (XmlException xex)
            {
                return null;
            }
            finally
            {
                xelement = null;
                YakShopStock = null;
            }
        }
        public class herds
        {
            public List<clsherd> herd { get; set; }
        }
        public object getherd(int T)
        {

            XElement xelement = null;
            IEnumerable<XElement> YakShopHerd = null;
            clsherd objherd = null;
            List<clsherd> objherdlist = null;
            herds objherds = null;
            try
            {
                if (File.Exists(HttpContext.Current.Server.MapPath("~/Database") + "/herd.xml"))
                {
                    xelement = XElement.Load(HttpContext.Current.Server.MapPath("~/Database") + "/herd.xml");
                    YakShopHerd = xelement.Elements();
                    objherdlist = new List<clsherd>();
                    objherds = new herds();
                    foreach (var herds in YakShopHerd)
                    {
                        objherd = new clsherd();
                        objherd.name = herds.Attribute("name").Value;
                        objherd.age_last_saved = Convert.ToDouble(herds.Attribute("age").Value);
                        objherd.age = Convert.ToDouble(herds.Attribute("age").Value) + Convert.ToDouble(T) / yakageStandarddays;
                        if (objherd.age <= yakageyears)
                        {
                            objherdlist.Add(objherd);
                        }
                    }
                }
                objherds.herd = objherdlist;
                return objherds;
            }
            catch (XmlException xex)
            {
                return null;
            }
            finally
            {
                xelement = null;
                YakShopHerd = null;
            }
        }
    }
    [Serializable]
    public class clsherd
    {
        public string name { get; set; }
        public Double age { get; set; }
        public Double age_last_saved { get; set; }
    }
    [Serializable]
    public class onlineOrder
    {
        public string customer { get; set; }
        public stock order { get; set; }
    }
    [Serializable]
    public class stock
    {
        public Double milk
        {
            set;
            get;
        }
        public int skins
        {
            set;
            get;
        }
        public Double getMilkStock(int noYak, int T, IEnumerable<XElement> age, int yakageStandarddays, int yakageyears)
        {
            IEnumerable<XElement> _currentage = age.Where(y => (Convert.ToDouble(y.Attribute("age").Value) * yakageStandarddays + (T - 1)) <= yakageyears * yakageStandarddays);
            double yakagesum = _currentage.Sum(h => Convert.ToDouble(h.Attribute("age").Value));
            return noYak * 50 * T - ((yakagesum) * yakageStandarddays * T + noYak * (T * (T - 1) / 2)) * 0.03;
        }
        public int getskinsStock(int noYak, int T, IEnumerable<XElement> age, int yakageStandarddays, int yakageyears)
        {
            int skins = 0;

            foreach (var h in age)
            {
                if (((Convert.ToDouble(h.Attribute("age").Value)) * yakageStandarddays + (T - 1)) <= yakageyears * yakageStandarddays && Convert.ToDouble(T - 1) > Convert.ToDouble(8 + ((Convert.ToDouble(h.Attribute("age").Value)) * yakageStandarddays + T - 1) * 0.01))
                {
                    skins = skins + Convert.ToInt32(Math.Floor(Convert.ToDouble(T - 1) / Convert.ToDouble(8 + ((Convert.ToDouble(h.Attribute("age").Value)) * yakageStandarddays) * 0.01)));
                }
            }
            return noYak + skins;
        }
    }
}