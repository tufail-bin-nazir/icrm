using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace icrm.Controllers
{
    public class CultureController : Controller
    {
        [Route("culture/setenglishculture/{act}/{cont}")]
        public ActionResult SetenglishCulture(String act, String cont)
        {
            Session["culture"] = "en-US";
            return RedirectToAction(act, cont);
        }

        [Route("culture/setarabicculture/{act}/{cont}")]
        public ActionResult SetarabicCulture(String act, String cont)
        {
            Session["culture"] = "ar-SA";
            return RedirectToAction(act, cont);
        }
    }
}