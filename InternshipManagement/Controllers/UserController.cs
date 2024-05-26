using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
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
                    return RedirectToAction("InstructorRegister");
                }
            }
            return View();
        }


        public ActionResult InstructorRegister()
        {
            return View("InstructorRegister");
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


                var existingStudent = new Student();
                existingStudent.FirstName = user.FirstName;
                existingStudent.LastName = user.FirstName;
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
        //Chức năng quên mật khẩu, reset mật khẩu thành 123456 rồi gửi email cho người dùng
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
            var user = data.Users.FirstOrDefault(x => x.Email == email);
            if (user == null)
            {
                ViewBag.Message = "Không tìm thấy người dùng với email này hoặc email không hợp lệ";
                return View();
            }
            else
            {
                // Tạo mã OTP ngẫu nhiên
                Random random = new Random();
                int otp = random.Next(100000, 999999); // OTP có 6 chữ số

                // Lưu mã OTP vào OTPStorage
                OTPStorage.AddOTP(email, otp.ToString());

                // Gửi mã OTP đến địa chỉ email của người dùng
                string subject = "Xác nhận đổi mật khẩu";
                string body = "Mã OTP của bạn là: " + otp.ToString() + ". Vui lòng sử dụng mã này để đổi mật khẩu.";
                bool emailSent = otpStorage.SendEmail(email, subject, body);

                if (emailSent)
                {
                    ViewBag.Message = "OTP đã được gửi đến địa chỉ email của bạn.";
                }
                else
                {
                    ViewBag.Message = "Đã xảy ra lỗi trong quá trình gửi email. Vui lòng thử lại sau.";
                }
            }
            return View();
        }
        [HttpGet]
        public ActionResult CheckOTP()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckOTP(string email, string otp)
        {
            // Kiểm tra xem mã OTP nhập vào có khớp với mã OTP đã gửi hay không
            string storedOTP = OTPStorage.GetOTP(email);
            if (storedOTP == null || storedOTP != otp)
            {
                ViewBag.Message = "Mã OTP không hợp lệ. Vui lòng kiểm tra lại.";
                return View();
            }
            else
            {
                // Xóa mã OTP khỏi OTPStorage sau khi sử dụng thành công
                OTPStorage.RemoveOTP(email);
                ViewBag.Email = email;
                return RedirectToAction("ResetPassword", new { resetEmail = email });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(string resetEmail, string newPassword, string confirmPassword)
        {
            var user = data.Users.FirstOrDefault(x => x.Email == resetEmail);
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

                    ViewBag.Message = "Đổi mật khẩu thành công.";
                    return RedirectToAction("Login"); // Hoặc chuyển hướng đến trang khác
                }
            }
        }
    }
}