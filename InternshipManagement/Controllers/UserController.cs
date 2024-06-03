using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Xml.Linq;
using InternshipManagement.Models;
using Microsoft.Ajax.Utilities;

namespace InternshipManagement.Controllers
{
    public class UserController : Controller
    {
        InternshipManagementEntities data = new InternshipManagementEntities();
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
                var user = data.Users.FirstOrDefault(u => u.UserID == userID);

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
        //Chức năng đăng nhập
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User users, string userName, string password)
        {
            if (ModelState.IsValid)
            {
                // Mã hóa mật khẩu
                string hashedPassword = PasswordHasher.HashPassword(password);
                var user = data.Users.FirstOrDefault(u => u.Username.Equals(userName) && u.Password.Equals(hashedPassword));

                if (user != null)
                {
                    // Lưu thông tin đăng nhập vào lịch sử
                    var loginHistory = new LoginHistory
                    {
                        UserID = user.UserID,
                        LoginTime = DateTime.Now,
                        IPAddress = Request.UserHostAddress // Lấy địa chỉ IP của người dùng
                    };

                    // Lưu lịch sử đăng nhập vào cơ sở dữ liệu
                    data.LoginHistories.Add(loginHistory);
                    data.SaveChanges();

                    // Thiết lập session cho người dùng đăng nhập thành công
                    Session["UserName"] = user.Username;
                    Session["UserID"] = user.UserID;

                    // Chuyển hướng người dùng đến trang tương ứng với vai trò của họ
                    if (user.RoleID == 1)
                    {
                        return RedirectToAction("Index", "Student");
                    }
                    else if (user.RoleID == 2)
                    {
                        return RedirectToAction("Index", "Instructor");
                    }
                }
                else
                {
                    ViewBag.Message = "Tên đăng nhập hoặc mật khẩu không đúng";
                    return View();
                }
            }
            return View(users);
        }

        // đăng ký
        public ActionResult Register(int? role)
        {
            if (role.HasValue)
            {
                if (role == 1)
                {
                    return RedirectToAction("RegisterEmail");
                }
                else if (role == 2)
                {
                    return RedirectToAction("RegisterEmail");
                }
            }
            return View();
        }
        //Chức năng đăng xuất
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        // Helper function to verify password
        private bool VerifyPassword(string inputPassword, string storedPasswordHash)
        {
            string inputPasswordHash = PasswordHasher.HashPassword(inputPassword);
            return inputPasswordHash == storedPasswordHash;
        }
        //Trang nhập Email
        [HttpGet]
        public ActionResult RegisterEmail()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterEmail(string email)
        {
            try
            {
                OTPStorage otpStorage = new OTPStorage();
                if (string.IsNullOrWhiteSpace(email))
                {
                    ModelState.AddModelError("Email", "Vui lòng nhập địa chỉ email.");
                    return View();
                }

                var checkEmail = data.Users.FirstOrDefault(c => c.Email == email);
                if (checkEmail != null)
                {
                    ModelState.AddModelError("Email", "Địa chỉ email đã được sử dụng.");
                    return View();
                }

                string otp = otpStorage.GenerateOTP();

                // Lưu mã OTP vào phiên làm việc
                Session["OTP"] = otp;
                Session["Email"] = email;

                // Gửi mã OTP qua email
                otpStorage.SendOTPByEmail(email, otp);

                // Chuyển hướng đến trang nhập mã OTP
                return RedirectToAction("EnterOTP", new { email });
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có
                ViewBag.ErrorMessage = "Đã xảy ra lỗi khi đăng ký tài khoản: " + ex.Message;
                return View();
            }
        }
        [HttpGet]
        public ActionResult EnterOTP()
        {
            // Kiểm tra xem Session["Email"] có null không
            if (Session["Email"] == null)
            {
                return RedirectToAction("RegisterEmail");
            }

            ViewBag.Email = Session["Email"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult EnterOTP(string otp1, string otp2, string otp3, string otp4, string otp5, string otp6)
        {
            try
            {
                string otp = otp1 + otp2 + otp3 + otp4 + otp5 + otp6;
                string storedOTP = (string)Session["OTP"];

                if (otp == storedOTP)
                {
                    return RedirectToAction("StudentRegister");
                }
                else
                {
                    ViewBag.ErrorMessage = "Mã OTP không hợp lệ.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có
                ViewBag.ErrorMessage = "Đã xảy ra lỗi khi nhập mã OTP: " + ex.Message;
                return View();
            }
        }
        //Trang đăng ký cho Student 
        [HttpGet]
        public ActionResult StudentRegister()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StudentRegister(User user)
        {
            if (ModelState.IsValid)
            {
                // Hash password
                user.Password = PasswordHasher.HashPassword(user.Password);
                user.Avatar = GenerateDefaultAvatar(user.FirstName);
                user.CreateDate = DateTime.Now;
                user.UpdateDate = DateTime.Now;
                user.RoleID = 1;
                user.Email = Session["Email"].ToString();

                // Disable validation on save for the current context
                data.Configuration.ValidateOnSaveEnabled = false;

                string email = Session["Email"].ToString();
                int atIndex = email.IndexOf('@');
                if (atIndex != -1)
                {
                    user.Username = email.Substring(0, atIndex);
                }

                var existingStudent = new Student
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DateOfBirth = user.DateOfBirth,
                    Email = user.Email,
                    Phone = user.Phone,
                    Avatar = user.Avatar,
                    Description = null,
                    CV = null,
                    Class = null,
                    UserID = user.UserID,
                    isActive = true,
                    isDelete = false,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now
                };
                data.Students.Add(existingStudent);

                // Add user to database
                data.Users.Add(user);
                data.SaveChanges();
                return RedirectToAction("Index", "Student");
            }
            return RedirectToAction("RegisterEmail");
        }

        //Trang đăng ký cho Instructor
        [HttpGet]
        public ActionResult RegisterInstructor()
        {
            ViewBag.Companies = new SelectList(data.Companies.ToList().OrderBy(r => r.CompanyID), "CompanyID", "CompanyName");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterInstructor(User user, int? CompanyID)
        {
            ViewBag.Companies = new SelectList(data.Companies.ToList().OrderBy(r => r.CompanyID), "CompanyID", "CompanyName");

            if (ModelState.IsValid)
            {
                // Hash password
                user.Password = PasswordHasher.HashPassword(user.Password);
                user.Avatar = GenerateDefaultAvatar(user.FirstName);
                user.CreateDate = DateTime.Now;
                user.UpdateDate = DateTime.Now;
                user.RoleID = 2;
                user.Email = Session["Email"].ToString();

                // Disable validation on save for the current context
                data.Configuration.ValidateOnSaveEnabled = false;

                string email = Session["Email"].ToString();
                int atIndex = email.IndexOf('@');
                if (atIndex != -1)
                {
                    user.Username = email.Substring(0, atIndex);
                }

                var existingInstructor = new Instructor
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DateOfBirth = user.DateOfBirth,
                    Email = user.Email,
                    Phone = user.Phone,
                    Avatar = user.Avatar,
                    Description = null,
                    CompanyID = CompanyID.Value,
                    UserID = user.UserID,
                    isActive = true,
                    isDelete = false,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now
                };
                data.Instructors.Add(existingInstructor);

                // Add user to database
                data.Users.Add(user);
                data.SaveChanges();
                return RedirectToAction("Index", "Instructor");
            }
            return RedirectToAction("RegisterEmail");
        }
        private string GenerateDefaultAvatar(string firstName)
        {
            int width = 400;
            int height = 400;

            using (Bitmap bitmap = new Bitmap(width, height))
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    Random random = new Random();
                    Color randomColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
                    graphics.Clear(randomColor);

                    using (Font font = new Font("Arial", 200, FontStyle.Bold, GraphicsUnit.Pixel))
                    {
                        using (SolidBrush brush = new SolidBrush(Color.White))
                        {
                            string initial = firstName[0].ToString();
                            SizeF textSize = graphics.MeasureString(initial, font);
                            PointF position = new PointF((width - textSize.Width) / 2, (height - textSize.Height) / 2);

                            graphics.DrawString(initial, font, brush, position);

                            string avatarFolder = Server.MapPath("~/Content/AvatarImages/");
                            // Ensure the folder exists
                            if (!Directory.Exists(avatarFolder))
                            {
                                Directory.CreateDirectory(avatarFolder);
                            }

                            string avatarFileName = $"{Guid.NewGuid()}.jpg";
                            string avatarFilePath = Path.Combine(avatarFolder, avatarFileName);
                            bitmap.Save(avatarFilePath, ImageFormat.Jpeg);

                            return avatarFileName;
                        }
                    }
                }
            }
        }

        //Chức năng đổi mật khẩu 
        [HttpGet]
        public ActionResult ChangePassword(int userID)
        {
            var user = data.Users.FirstOrDefault(u => u.UserID == userID);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(int userID, string oldPassword, string newPassword, string confirmPassword)
        {
            User user = data.Users.FirstOrDefault(u => u.UserID == userID);
            if (user == null)
            {
                return HttpNotFound();
            }
            else
            {
                if (!VerifyPassword(oldPassword, user.Password))
                {
                    ViewBag.ThongBao = "Mật khẩu hiện tại không đúng";
                    return View();
                }
                // Validate new password
                if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
                {
                    ViewBag.ThongBao = "Mật khẩu mới phải có ít nhất 6 ký tự";
                    return View();
                }
                // Confirm new password
                if (newPassword != confirmPassword)
                {
                    ViewBag.ThongBao = "Nhập lại mật khẩu mới không khớp";
                    return View();
                }
                // Update user password
                user.Password = PasswordHasher.HashPassword(newPassword);
                data.SaveChanges();

                ViewBag.ThongBao = "Đổi mật khẩu thành công";
                return View();
            }
        }
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string email)
        {
            OTPStorage otpStorage = new OTPStorage();
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("Email", "Vui lòng nhập địa chỉ email.");
                return View();
            }

            var user = data.Users.FirstOrDefault(x => x.Email == email);
            if (user == null)
            {
                ViewBag.Message = "Không tìm thấy người dùng với email này hoặc email không hợp lệ";
                return View();
            }

            string otp = otpStorage.GenerateOTP();

            // Lưu mã OTP vào phiên làm việc
            Session["OTP"] = otp;
            Session["Email"] = email;
            

            // Gửi mã OTP đến địa chỉ email của người dùng
            string subject = "Xác nhận đổi mật khẩu";
            string body = "Mã OTP của bạn là: " + otp.ToString() + ". Vui lòng sử dụng mã này để đổi mật khẩu.";
            bool emailSent = otpStorage.SendEmail(email, subject, body);

            // Chuyển hướng đến trang nhập mã OTP
            return RedirectToAction("CheckOTP");
        }

        [HttpGet]
        public ActionResult CheckOTP()
        {
            ViewBag.Email = (string)Session["Email"];
            return View();
        }

        [HttpPost]
        public ActionResult CheckOTP(string otp1, string otp2, string otp3, string otp4, string otp5, string otp6)
        {
            try
            {
                string otp = otp1 + otp2 + otp3 + otp4 + otp5 + otp6;
                string storedOTP = Session["OTP"] as string;

                if (otp == storedOTP)
                {
                    return RedirectToAction("ResetPassword");
                }
                else
                {
                    ViewBag.ErrorMessage = "Mã OTP không hợp lệ.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có
                ViewBag.ErrorMessage = "Đã xảy ra lỗi khi nhập mã OTP: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public ActionResult EditCV()
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (Session["UserID"] == null)
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Login");
            }

            int userID = (int)Session["UserID"];
            var user = data.Users.FirstOrDefault(u => u.UserID == userID);
            var profile = data.Profiles.FirstOrDefault(p => p.UserID == userID);

            if (user == null)
            {
                return HttpNotFound();
            }

            var viewModel = new EditCVViewModel
            {
                User = user,
                Profile = profile
            };

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCV(EditCVViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                int userID = (int)Session["UserID"];

                var user = data.Users.Find(userID);
                var profile = data.Profiles.FirstOrDefault(p => p.UserID == userID);

                if (user == null)
                {
                    return HttpNotFound();
                }

                // Update user details if they are not null or empty
                if (!string.IsNullOrEmpty(viewModel.User.FirstName))
                    user.FirstName = viewModel.User.FirstName;

                if (!string.IsNullOrEmpty(viewModel.User.LastName))
                    user.LastName = viewModel.User.LastName;

                if (viewModel.User.DateOfBirth != null && viewModel.User.DateOfBirth != new DateTime(0001, 01, 01))
                    user.DateOfBirth = viewModel.User.DateOfBirth;

                if (!string.IsNullOrEmpty(viewModel.User.Phone))
                    user.Phone = viewModel.User.Phone;

                if (!string.IsNullOrEmpty(viewModel.User.Email))
                    user.Email = viewModel.User.Email;

                if (!string.IsNullOrEmpty(viewModel.User.Avatar))
                    user.Avatar = viewModel.User.Avatar;

                if (!string.IsNullOrEmpty(viewModel.User.Address))
                    user.Address = viewModel.User.Address;

                if (!string.IsNullOrEmpty(viewModel.User.Gender))
                    user.Gender = viewModel.User.Gender;

                user.UpdateDate = DateTime.Now;

                // Handle password change
                if (!string.IsNullOrEmpty(viewModel.OldPassword) || !string.IsNullOrEmpty(viewModel.NewPassword) || !string.IsNullOrEmpty(viewModel.ConfirmPassword))
                {
                    if (!VerifyPassword(viewModel.OldPassword, user.Password))
                    {
                        ModelState.AddModelError("OldPassword", "Mật khẩu hiện tại không đúng");
                        return View(viewModel);
                    }

                    if (string.IsNullOrEmpty(viewModel.NewPassword) || viewModel.NewPassword.Length < 6)
                    {
                        ModelState.AddModelError("NewPassword", "Mật khẩu phải trên 6 kí tự");
                        return View(viewModel);
                    }

                    if (viewModel.NewPassword != viewModel.ConfirmPassword)
                    {
                        ModelState.AddModelError("ConfirmPassword", "Mật khẩu mới không khớp với mật khẩu cũ");
                        return View(viewModel);
                    }

                    user.Password = PasswordHasher.HashPassword(viewModel.NewPassword);
                }

                // Update profile details
                if (profile == null)
                {
                    profile = new Profile
                    {
                        UserID = userID,
                        CreatedAt = DateTime.Now
                    };
                    data.Profiles.Add(profile);
                }
                else
                {
                    if (!string.IsNullOrEmpty(viewModel.Profile.Bio))
                        profile.Bio = viewModel.Profile.Bio;

                    if (!string.IsNullOrEmpty(viewModel.Profile.Skills))
                        profile.Skills = viewModel.Profile.Skills;

                    if (!string.IsNullOrEmpty(viewModel.Profile.Experience))
                        profile.Experience = viewModel.Profile.Experience;

                    if (!string.IsNullOrEmpty(viewModel.Profile.Certifications))
                        profile.Certifications = viewModel.Profile.Certifications;

                    if (!string.IsNullOrEmpty(viewModel.Profile.Website))
                        profile.Website = viewModel.Profile.Website;
                }
                // Handle avatar image upload
                if (viewModel.AvatarImage != null && viewModel.AvatarImage.ContentLength > 0)
                {
                    string fileExtension = Path.GetExtension(viewModel.AvatarImage.FileName);
                    string fileName = Guid.NewGuid().ToString() + fileExtension;
                    string path = Path.Combine(Server.MapPath("~/Content/AvatarImages"), fileName);
                    viewModel.AvatarImage.SaveAs(path);
                    user.Avatar = fileName;
                }
                else
                {
                    user.Avatar = user.Avatar; 
                }

                data.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật thông tin thành công";

                return RedirectToAction("EditCV");
            }

            return View(viewModel);
        }
        public ActionResult Profile(int id)
        {
            var user = data.Users.FirstOrDefault(u => u.UserID == id);
            var profile = data.Profiles.FirstOrDefault(p => p.UserID == id);

            if (user == null)
            {
                return HttpNotFound();
            }

            var viewModel = new EditCVViewModel
            {
                User = user,
                Profile = profile
            };

            return View(viewModel);
        }
        public PartialViewResult LoadCVModal(int id)
        {
            var profile = data.Profiles.FirstOrDefault(p => p.UserID == id);
            return PartialView("_CVModalPartial", profile);
        }


        [HttpGet]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(string newPassword, string confirmPassword)
        {
            string email = (string)Session["Email"];
            var user = data.Users.FirstOrDefault(x => x.Email == email);    
            if (user == null)
            {
                ViewBag.Message = "Không tìm thấy người dùng với email này hoặc email không hợp lệ";
                return View();
            }
            else
            {
                if (newPassword != confirmPassword)
                {
                    ViewBag.Message = "Mật khẩu không trùng khớp. Vui lòng nhập lại";
                    return View();
                }
                else
                {
                    // Hash mật khẩu mới trước khi lưu vào cơ sở dữ liệu
                    user.Password = PasswordHasher.HashPassword(newPassword);

                    // Lưu thay đổi vào cơ sở dữ liệu
                    data.SaveChanges();

                    ViewBag.Message = "Đổi mật khẩu thành công. Mật khẩu mới đã được gửi đến email của bạn.";
                    return RedirectToAction("Login"); // Hoặc chuyển hướng đến trang khác
                }
            }
        }
        // Method to view notifications
        public ActionResult Notifications()
        {
            int userId = Convert.ToInt32(Session["UserID"]);

            var notifications = data.Notifications
                                    .Where(n => n.ReceiverID == userId)
                                    .OrderByDescending(n => n.NotificationDateTime)
                                    .ToList();

            return View(notifications);
        }

        // Optionally, mark notifications as read
        [HttpPost]
        public ActionResult MarkAsRead(int notificationId)
        {
            var notification = data.Notifications.SingleOrDefault(n => n.NotificationID == notificationId);

            if (notification != null)
            {
                notification.IsRead = true;
                notification.UpdateDate = DateTime.Now;
                data.SaveChanges();
            }

            return RedirectToAction("Notifications");
        }
        public ActionResult LatestNotifications()
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            var notifications = data.Notifications
                                    .Where(n => n.ReceiverID == userId && !n.IsRead)
                                    .OrderByDescending(n => n.NotificationDateTime)
                                    .Take(5)
                                    .ToList();
            return PartialView("_LatestNotifications", notifications);
        }


        public ActionResult UnreadNotificationsCount()
        {
            if (Session["UserID"] == null)
            {
                return Json(new { redirectToLogin = true }, JsonRequestBehavior.AllowGet);
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            int unreadCount = data.Notifications
                                  .Count(n => n.ReceiverID == userId && !n.IsRead);

            return Json(new { count = unreadCount }, JsonRequestBehavior.AllowGet);
        }

    }
}