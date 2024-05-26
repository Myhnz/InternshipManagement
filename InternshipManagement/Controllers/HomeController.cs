using InternshipManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InternshipManagement.Controllers
{
    public class HomeController : Controller
    {
        InternshipManagementEntities hData = new InternshipManagementEntities();
        public ActionResult Index()
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (HttpContext.Session["UserID"] == null)
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Login", "User");
            }
            else
            {
                int userID = (int)Session["UserID"]; // Ép kiểu Session["UserID"] về kiểu int

                // Kiểm tra xem UserID có tồn tại trong bảng Users không
                var user = hData.Users.FirstOrDefault(u => u.UserID == userID);

                if (user != null)
                {
                    // Kiểm tra xem user có role là 1 hoặc 2 không
                    if (user.RoleID == 1 || user.RoleID == 2)
                    {
                        // Redirect đến action tương ứng cho role 1 hoặc 2
                        if (user.RoleID == 1)
                        {
                            return RedirectToAction("Index", "Student");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Instructor");
                        }
                    }
                    else
                    {
                        return RedirectToAction("Logout", "User");
                    }
                }
                else
                {
                    return RedirectToAction("Logout", "User");
                }
            }
        }

    }
}