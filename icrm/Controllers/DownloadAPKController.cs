using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace icrm.Controllers
{
    public class DownloadAPKController : Controller
    {
        // GET: DownloadAPK
        public ActionResult Index()
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath(@"~/App_Data/app-debug.apk"));
            string fileName = "icrm.apk";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
    }
}