using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InternshipManagement.Models
{
    public class RequireLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session["UserID"] == null)
            {
                // Nếu Session["UserID"] không tồn tại, chuyển hướng sang một controller khác
                filterContext.Result = new RedirectResult("~/Home/Index"); 
            }
        }
    }
}