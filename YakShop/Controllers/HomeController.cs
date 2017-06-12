using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using YakShop.Models;
using System.Web.Script.Serialization;
using static YakShop.Models.clsWebShop;

namespace YakShop.Controllers
{
    public class HomeController : Controller
    {
        // GET: Upload  
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult UploadFile()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            int T = 0;
            try
            {
                if (file.ContentLength > 0 && file.ContentType == "text/xml")
                {
                    T = Convert.ToInt32(HttpContext.Request["text"].ToString()==string.Empty?"0": HttpContext.Request["text"].ToString());
                    string _FileName = Path.GetFileName(file.FileName);
                    string _path = Path.Combine(Server.MapPath("~/Database"), _FileName);
                    file.SaveAs(_path);
                }
                using( WebClient wb=new WebClient())
                {
                    string apiresponse = string.Empty;
                    apiresponse = wb.DownloadString(HttpContext.Request.Url.AbsoluteUri.Replace(HttpContext.Request.Url.AbsolutePath, string.Empty) + @"/yak-shop/stock/" + T.ToString());
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var c = js.Deserialize<stock>(apiresponse);
                    apiresponse = wb.DownloadString(HttpContext.Request.Url.AbsoluteUri.Replace(HttpContext.Request.Url.AbsolutePath, string.Empty) + @"/yak-shop/herd/" + T.ToString());
                    var h = js.Deserialize<herds>(apiresponse);
                    string stock= "<b>In Stock </b>:</br>" + c.milk.ToString() + " liters of milk </br>" + c.skins.ToString() + " skins of wool</br>"; 
                    string herd = "<b>Herd:</b> </br>";
                    foreach (var he in h.herd)
                    {
                        herd += he.name +" " + he.age.ToString() +" years old</br>";
                    }
                    ViewBag.Message = stock + herd;
                }
                //ViewBag.Message = "File Uploaded Successfully!!";
                return View("Index");
            }
            catch
            {
                ViewBag.Message = "File upload failed!!";
                return View("Index");
            }
        }
    }
}
