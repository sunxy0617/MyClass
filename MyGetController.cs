using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace StudentManagement_Server.Controllers
{
    /// <summary>
    /// 支持Get
    /// </summary>
    public class MyGetController:Controller
    {
        protected new JsonResult Json(object data)
        {
            return Json(data, (string)null, (Encoding)null, JsonRequestBehavior.AllowGet);
        }
    }
}