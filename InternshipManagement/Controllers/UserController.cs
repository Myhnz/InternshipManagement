using System;
using System.Collections.Generic;
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
                //Mã hóa mật khẩu
                string hashedPassword = PasswordHasher.HashPassword(password);
                var user = data.Users.Where(u => u.Username.Equals(userName) && u.Password.Equals(hashedPassword)).ToList();
                if (user.Count() > 0)
                {
                    Session["UserName"] = user.FirstOrDefault().Username;
                    Session["UserID"] = user.FirstOrDefault().UserID;

                    var userID = Session["UserID"];
                    var userRole = user.FirstOrDefault().RoleID;
                    if (userRole == 1)
                    {
                        return RedirectToAction("Index", "Student");
                    }
                    if (userRole == 2)
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
                user.Avatar = "default.jpg";
                user.CreateDate = DateTime.Now;
                user.UpdateDate = DateTime.Now;
                user.RoleID = 1;
                user.Email = Session["Email"].ToString();

                // Disable validation on save for the current context
                data.Configuration.ValidateOnSaveEnabled = false;
                // Assuming Session["Email"] is a valid string
                string email = Session["Email"].ToString();

                // Find the index of '@' character
                int atIndex = email.IndexOf('@');
                // Check if the '@' character is found
                if (atIndex != -1)
                {
                    // Extract the substring from the beginning of the email string up to (but not including) the '@' character
                    string username = email.Substring(0, atIndex);

                    // Now username contains the part of the email before the '@' character
                    user.Username = username;
                }

                var existingStudent = new Student();
                existingStudent.FirstName = user.FirstName;
                existingStudent.LastName = user.LastName;
                existingStudent.DateOfBirth = user.DateOfBirth;
                existingStudent.Email = user.Email;
                existingStudent.Phone = user.Phone;
                existingStudent.Avatar = user.Avatar;
                existingStudent.Description = null;
                existingStudent.CV = null;
                existingStudent.Class = null;
                existingStudent.UserID = user.UserID;
                existingStudent.isActive = true;
                existingStudent.isDelete = false;
                existingStudent.CreateDate = DateTime.Now;
                existingStudent.UpdateDate = DateTime.Now;
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
                user.Avatar = "default.jpg";
                user.CreateDate = DateTime.Now;
                user.UpdateDate = DateTime.Now;
                user.RoleID = 2;
                user.Email = Session["Email"].ToString();

                // Disable validation on save for the current context
                data.Configuration.ValidateOnSaveEnabled = false;
                // Assuming Session["Email"] is a valid string
                string email = Session["Email"].ToString();

                // Find the index of '@' character
                int atIndex = email.IndexOf('@');
                // Check if the '@' character is found
                if (atIndex != -1)
                {
                    // Extract the substring from the beginning of the email string up to (but not including) the '@' character
                    string username = email.Substring(0, atIndex);

                    // Now username contains the part of the email before the '@' character
                    user.Username = username;
                }

                var existingInstructor = new Instructor();
                existingInstructor.FirstName = user.FirstName;
                existingInstructor.LastName = user.LastName;
                existingInstructor.DateOfBirth = user.DateOfBirth;
                existingInstructor.Email = user.Email;
                existingInstructor.Phone = user.Phone;
                existingInstructor.Avatar = user.Avatar;
                existingInstructor.Description = null;
                existingInstructor.CompanyID = CompanyID.Value;
                existingInstructor.UserID = user.UserID;
                existingInstructor.isActive = true;
                existingInstructor.isDelete = false;
                existingInstructor.CreateDate = DateTime.Now;
                existingInstructor.UpdateDate = DateTime.Now;
                data.Instructors.Add(existingInstructor);

                // Add user to database
                data.Users.Add(user);
                data.SaveChanges();
                return RedirectToAction("Index", "Instructor");
            }
            return RedirectToAction("RegisterEmail");
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

            if (user == null || profile == null)
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

                // Cập nhật các thuộc tính của User
                user.FirstName = viewModel.User.FirstName;
                user.LastName = viewModel.User.LastName;
                user.DateOfBirth = viewModel.User.DateOfBirth;
                user.Phone = viewModel.User.Phone;
                user.Email = viewModel.User.Email;
                user.Avatar = viewModel.User.Avatar;
                user.Address = viewModel.User.Address;
                user.Gender = viewModel.User.Gender;
                user.Password = viewModel.User.Password;
                user.UpdateDate = DateTime.Now;
                // Cập nhật các thuộc tính khác của User

                // Cập nhật các thuộc tính của Profile
                if (profile == null)
                {
                    profile = new Profile
                    {
                        UserID = userID,
                        CreatedAt = DateTime.Now
                    };
                    data.Profiles.Add(profile);
                }

                profile.Bio = viewModel.Profile.Bio;
                profile.Skills = viewModel.Profile.Skills;
                profile.Experience = viewModel.Profile.Experience;
                profile.Certifications = viewModel.Profile.Certifications;
                profile.Website = viewModel.Profile.Website;

                data.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            return View(viewModel);
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
    }
}