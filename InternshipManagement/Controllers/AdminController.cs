using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;
using InternshipManagement.Models;
using System.Drawing;
using System.Drawing.Imaging;
using static System.Net.WebRequestMethods;
using System.Web.Helpers;

namespace InternshipManagement.Controllers
{
    public class AdminController : Controller
    {
        InternshipManagementEntities aData = new InternshipManagementEntities();

        // Method to add error if a string is null or whitespace
        public void AddErrorIfNullOrWhitespace(string fieldName, string value, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                ModelState.AddModelError(fieldName, errorMessage);
            }
        }
        private bool IsUsernameExists(string username)
        {
            return aData.Users.Any(u => u.Username == username);
        }
        [HttpPost]
        public ActionResult CheckErrors(FormData formData, List<string> fieldsToCheck)
        {
            // Dictionary to store field errors
            Dictionary<string, string> errors = new Dictionary<string, string>();

            foreach (string fieldName in fieldsToCheck)
            {
                switch (fieldName)
                {
                    case "Username":
                        AddErrorIfNullOrWhitespace("Username", formData.Username, "Vui lòng nhập tên đăng nhập.");
                        if (IsUsernameExists(formData.Username))
                        {
                            ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                        }
                        break;
                    case "Password":
                        AddErrorIfNullOrWhitespace("Password", formData.Password, "Vui lòng nhập mật khẩu.");
                        break;
                    case "CompanyName":
                        AddErrorIfNullOrWhitespace("CompanyName", formData.CompanyName, "Vui lòng nhập tên công ty.");
                        break;
                    case "FirstName":
                        AddErrorIfNullOrWhitespace("FirstName", formData.FirstName, "Vui lòng nhập tên.");
                        break;
                    case "LastName":
                        AddErrorIfNullOrWhitespace("LastName", formData.LastName, "Vui lòng nhập họ.");
                        break;
                    case "Email":
                        AddErrorIfNullOrWhitespace("Email", formData.Email, "Vui lòng nhập địa chỉ email.");
                        break;
                    case "Phone":
                        AddErrorIfNullOrWhitespace("Phone", formData.Phone, "Vui lòng nhập số điện thoại.");
                        break;
                    case "Address":
                        AddErrorIfNullOrWhitespace("Address", formData.Address, "Vui lòng nhập địa chỉ.");
                        break;
                    case "Gender":
                        AddErrorIfNullOrWhitespace("Gender", formData.Gender, "Vui lòng nhập giới tính.");
                        break;
                    case "DateOfBirth":
                        if (formData.DateOfBirth == null)
                        {
                            ModelState.AddModelError("DateOfBirth", "Vui lòng nhập ngày sinh.");
                        }
                        break;
                    case "RoleID":
                        if (formData.RoleID == null)
                        {
                            ModelState.AddModelError("RoleID", "Vui lòng chọn vai trò.");
                        }
                        break;
                    default:
                        break;
                }
            }

            // Check if there are any model state errors
            if (!ModelState.IsValid)
            {
                // Convert model state errors to dictionary format
                errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
                );
            }

            // Return JSON containing errors
            return Json(errors);
        }




        //Trang chủ Admin
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Dashboard()
        {
            return View();
        }
        public ActionResult headerMenu()
        {
            return PartialView("_adminMenu");
        }
        //Chức năng đăng nhập
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Administrator admins, string userName, string password)
        {
            if (ModelState.IsValid)
            {
                //Mã hóa mật khẩu
                string hashedPassword = PasswordHasher.HashPassword(password);
                var admin = aData.Administrators.Where(u => u.Username.Equals(userName) && u.Password.Equals(hashedPassword)).ToList();
                if (admin.Count() > 0)
                {
                    //Session["UserName"] = admin.FirstOrDefault().Username;
                    Session["AdminID"] = admin.FirstOrDefault().AdminID;
                    int adminID = (int)Session["AdminID"];
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Message = "Tên đăng nhập hoặc mật khẩu không đúng";
                    return View();
                }
            }
            return View(admins);
        }
        //Chức năng đăng xuất
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "Admin");
        }

        //Chức năng Quản lý Quản trị viên
        public ActionResult Admins()
        {
            var admins = aData.Administrators.OrderBy(a => a.AdminID).ToList();
            return View(admins);
        }
        //Trang quên mật khẩu nhập email để xác thực
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

            var user = aData.Administrators.FirstOrDefault(x => x.Email == email);
            if (user == null)
            {
                ViewBag.Message = "Không tìm thấy người dùng với email này hoặc email không hợp lệ";
                return View();
            }

            string otp = otpStorage.GenerateOTP();

            // Lưu mã OTP vào phiên làm việc
            Session["OTP"] = otp;
            Session["Email"] = email;
            //Session["UserID"] = user.UserID;

            // Gửi mã OTP đến địa chỉ email của người dùng
            string subject = "Xác nhận đổi mật khẩu";
            string body = "Mã OTP của bạn là: " + otp.ToString() + ". Vui lòng sử dụng mã này để đổi mật khẩu.";
            bool emailSent = otpStorage.SendEmail(email, subject, body);

            // Chuyển hướng đến trang nhập mã OTP
            return RedirectToAction("CheckOTP");
        }
        //Trang nhập OTP để xác thực 
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
        //Trang reset mật khẩu
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
            var user = aData.Administrators.FirstOrDefault(x => x.Email == email);
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
                    aData.SaveChanges();

                    ViewBag.Message = "Đổi mật khẩu thành công. Mật khẩu mới đã được gửi đến email của bạn.";
                    return RedirectToAction("Login"); // Hoặc chuyển hướng đến trang khác
                }
            }
        }
        //Chức năng tạo tài khoản cho Quản trị viên
        [HttpGet]
        public ActionResult CreateAdminAccount()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAdminAccount(Administrator admin)
        {// Thêm thông báo lỗi vào ModelState cho các trường bỏ trống
            if (string.IsNullOrWhiteSpace(admin.Username))
            {
                ModelState.AddModelError("Username", "Vui lòng nhập tên đăng nhập.");
            }
            if (string.IsNullOrWhiteSpace(admin.Password))
            {
                ModelState.AddModelError("Password", "Vui lòng nhập mật khẩu.");
            }
            if (string.IsNullOrWhiteSpace(admin.FirstName))
            {
                ModelState.AddModelError("FirstName", "Vui lòng nhập tên.");
            }
            if (string.IsNullOrWhiteSpace(admin.LastName))
            {
                ModelState.AddModelError("LastName", "Vui lòng nhập họ.");
            }
            if (admin.DateOfBirth == null)
            {
                ModelState.AddModelError("DateOfBirth", "Vui lòng nhập ngày sinh.");
            }
            if (string.IsNullOrWhiteSpace(admin.Email))
            {
                ModelState.AddModelError("Email", "Vui lòng nhập địa chỉ email.");
            }
            if (string.IsNullOrWhiteSpace(admin.Phone))
            {
                ModelState.AddModelError("Phone", "Vui lòng nhập số điện thoại.");
            }
            if (ModelState.IsValid)
            {
                var checkAdminName = aData.Administrators.FirstOrDefault(c => c.Username == admin.Username);

                if (checkAdminName == null)
                {
                    //Hash mật khẩu
                    admin.Password = PasswordHasher.HashPassword(admin.Password);
                    aData.Configuration.ValidateOnSaveEnabled = false;
                    aData.Administrators.Add(admin);
                    aData.SaveChanges();
                    return RedirectToAction("Admins");
                }
                else
                {
                    ViewBag.MessageForUserName = "Tên đăng nhập đã tồn tại";
                }
            }
            return View(admin);
        }
        //Chức năng chỉnh sửa quản trị viên
        [HttpGet]
        public ActionResult EditAdminAccount(int adminID)
        {
            var existingAdmin = aData.Administrators.SingleOrDefault(e => e.AdminID == adminID);
            if (existingAdmin == null)
            {
                return HttpNotFound();
            }
            return View(existingAdmin);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAdminAccount(int adminID, Administrator admin)
        {
            var updateAdmin = aData.Administrators.SingleOrDefault(u => u.AdminID == adminID);
            if (updateAdmin != null)
            {
                updateAdmin.FirstName = admin.FirstName;
                updateAdmin.LastName = admin.LastName;
                updateAdmin.Password = admin.Password;
                updateAdmin.Phone = admin.Phone;
                updateAdmin.DateOfBirth = admin.DateOfBirth;
                updateAdmin.Email = admin.Email;
                aData.SaveChanges();
                return RedirectToAction("Admins");
            }
            return View(updateAdmin);
        }
        //Trang chi tiết quản trị viên
        public ActionResult DetailAdminAccount(int adminID)
        {
            var existingAdmin = aData.Administrators.SingleOrDefault(e => e.AdminID == adminID);
            if (existingAdmin == null)
            {
                return HttpNotFound();
            }
            return View(existingAdmin);
        }
        //Chức năng xóa quản trị viên
        [HttpGet]
        public ActionResult DeleteAdminAccount(int adminID)
        {
            var existingAdmin = aData.Administrators.SingleOrDefault(e => e.AdminID == adminID);
            if (existingAdmin == null)
            {
                return HttpNotFound();
            }
            return View(existingAdmin);
        }
        //Chức năng Quản lý người dùng
        public ActionResult Users()
        {
            return View();
        }
        [HttpPost]
        public JsonResult GetUsers(DataTableParameters parameters, string sortColumn, string sortDirection)
        {
            int draw = parameters.Draw;
            int start = parameters.Start;
            int length = parameters.Length;
            string searchValue = parameters.Search.Value;

            IQueryable<User> query = aData.Users;

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(u => u.FirstName.Contains(searchValue) ||
                                         u.LastName.Contains(searchValue) ||
                                         u.Username.Contains(searchValue) ||
                                         u.Email.Contains(searchValue) ||
                                         u.Phone.Contains(searchValue));
            }
            if (!string.IsNullOrEmpty(sortColumn))
            {
                switch (sortColumn)
                {
                    case "FirstName":
                        query = sortDirection == "asc" ? query.OrderBy(u => u.FirstName) : query.OrderByDescending(u => u.FirstName);
                        break;
                    case "LastName":
                        query = sortDirection == "asc" ? query.OrderBy(u => u.LastName) : query.OrderByDescending(u => u.LastName);
                        break;
                    case "Username":
                        query = sortDirection == "asc" ? query.OrderBy(u => u.Username) : query.OrderByDescending(u => u.Username);
                        break;
                    case "Email":
                        query = sortDirection == "asc" ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email);
                        break;
                    case "Phone":
                        query = sortDirection == "asc" ? query.OrderBy(u => u.Phone) : query.OrderByDescending(u => u.Phone);
                        break;
                    default:
                        query =  query.OrderByDescending(u => u.UpdateDate);
                        break;
                }
            }

            int recordsTotal = aData.Users.Count();

            if (length == -1) // Kiểm tra nếu chọn "All"
            {
                length = recordsTotal; // Thiết lập độ dài là tổng số bản ghi
            }

            int recordsFiltered = !string.IsNullOrEmpty(searchValue) ? query.Count() : recordsTotal;

            var data = query.ToList().Skip(start).Take(length).ToList();


            // Tính toán số thứ tự
            int sequenceNumber = start + 1;

            var formattedData = data.Select(u => new
            {
                SequenceNumber = sequenceNumber++,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Username = u.Username,
                Email = u.Email,
                Phone = u.Phone
            });

            return Json(new
            {
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = formattedData
            });
        }



        //Chức năng tạo tài khoản cho người dùng (Sinh viên, Giảng viên)
        [HttpGet]
        public ActionResult CreateUserAccount()
        {
            ViewBag.Roles = new SelectList(aData.UserRoles.ToList().OrderBy(r => r.RoleName), "RoleID", "RoleName");
            ViewBag.Companies = new SelectList(aData.Companies.ToList().OrderBy(r => r.CompanyID), "CompanyID", "CompanyName");
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUserAccount(User user, int? CompanyID)
        {
            try
            {
                ViewBag.Roles = new SelectList(aData.UserRoles.ToList().OrderBy(r => r.RoleID), "RoleID", "RoleName");
                ViewBag.Companies = new SelectList(aData.Companies.ToList().OrderBy(r => r.CompanyID), "CompanyID", "CompanyName");

                if (ModelState.IsValid)
                {
                    user.Password = PasswordHasher.HashPassword(user.Password);
                    user.Avatar = GenerateDefaultAvatar(user.FirstName);
                    user.CreateDate = DateTime.Now;
                    user.UpdateDate = DateTime.Now;

                    aData.Configuration.ValidateOnSaveEnabled = false;

                    if (user.RoleID == 1) // Sinh viên
                    {
                        var existingStudent = new Student
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            DateOfBirth = null,
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
                        aData.Students.Add(existingStudent);
                    }
                    else if (user.RoleID == 2) // Giảng viên
                    {
                        var existingInstructor = new Instructor
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            DateOfBirth = null,
                            Avatar = user.Avatar,
                            Description = null,
                            CompanyID = CompanyID.Value,
                            UserID = user.UserID,
                            isActive = true,
                            isDelete = false,
                            CreateDate = DateTime.Now,
                            UpdateDate = DateTime.Now
                        };
                        aData.Instructors.Add(existingInstructor);
                    }

                    aData.Users.Add(user);
                    aData.SaveChanges();
                    OTPStorage otpStorage = new OTPStorage();
                    string userEmail = user.Email;
                    // Gửi email thông báo tài khoản và mật khẩu
                    string subject = "Thông tin tài khoản của bạn";
                    string body = $"Tài khoản của bạn đã được tạo thành công. Tên đăng nhập: {user.Username}, Mật khẩu: {user.Password}";
                    otpStorage.SendEmail(userEmail, subject, body);
                    return RedirectToAction("Users");
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi thêm người dùng: " + ex.Message });
            }

            return RedirectToAction("Users");
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

                            string avatarFileName = $"{Guid.NewGuid()}.jpg";
                            string avatarFilePath = Server.MapPath($"~/Content/AvatarImages/{avatarFileName}");
                            bitmap.Save(avatarFilePath, ImageFormat.Jpeg);

                            return avatarFileName;
                        }
                    }
                }
            }
        }


        // Chức năng chỉnh sửa Người dùng
        [HttpGet]
        public ActionResult EditUserAccount(string username)
        {
            var existingUser = aData.Users.SingleOrDefault(c => c.Username == username);
            if (existingUser == null)
            {
                return HttpNotFound();
            }
            ViewBag.Roles = new SelectList(aData.UserRoles.ToList().OrderBy(r => r.RoleID), "RoleID", "RoleName");
            return PartialView( existingUser); // Return partial view for modal
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUserAccount(User user, HttpPostedFileBase avatarFile)
        {
            try
            {
                var updateUser = aData.Users.SingleOrDefault(u => u.UserID == user.UserID);
                ViewBag.Roles = new SelectList(aData.UserRoles.ToList().OrderBy(r => r.RoleID), "RoleID", "RoleName");
                if (updateUser != null)
                {
                    updateUser.FirstName = user.FirstName;
                    updateUser.LastName = user.LastName;
                    updateUser.DateOfBirth = user.DateOfBirth;
                    updateUser.Gender = user.Gender;
                    updateUser.Email = user.Email;
                    updateUser.Phone = user.Phone;

                    // Process the avatar file
                    if (avatarFile != null && avatarFile.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(avatarFile.FileName);
                        string path = Path.Combine(Server.MapPath("~/Materials/AvatarImages"), fileName);
                        avatarFile.SaveAs(path); // Save the file to the server
                        updateUser.Avatar = fileName; // Save the file name to the database
                    }

                    updateUser.UpdateDate = DateTime.Now;
                    aData.SaveChanges();
                    return RedirectToAction("Users");
                }
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                return Content("Có lỗi xảy ra trong quá trình chỉnh sửa tài khoản người dùng.");
            }
        }


        // Trang thông tin Người dùng
        public ActionResult DetailUserAccount(string username)
        {
            var existingUser = aData.Users.SingleOrDefault(c => c.Username == username);
            if (existingUser == null)
            {
                return HttpNotFound();
            }
            return PartialView(existingUser);
        }


        //Chức năng xóa người dùng
        public ActionResult DeleteUserAccount(int userID)
        {
            var deleteUser = aData.Users.SingleOrDefault(c => c.UserID == userID);
            if (deleteUser != null)
            {
                var checkDeletedUser = aData.DeletedUsers.FirstOrDefault(d => d.Username == deleteUser.Username);
                if (checkDeletedUser == null)
                {
                    DeletedUser deletedUser = new DeletedUser();
                    deletedUser.FirstName = deleteUser.FirstName;
                    deletedUser.LastName = deleteUser.LastName;
                    deletedUser.Username = deleteUser.Username;
                    deletedUser.Password = deleteUser.Password;
                    deletedUser.DateOfBirth = deleteUser.DateOfBirth;
                    deletedUser.Avatar = deleteUser.Avatar;
                    deletedUser.Gender = deleteUser.Gender;
                    deletedUser.Email = deleteUser.Email;
                    deletedUser.Phone = deleteUser.Phone;
                    deletedUser.Address = deleteUser.Address;
                    deletedUser.RoleID = deleteUser.RoleID;
                    deletedUser.DeletedDate = DateTime.Now;
                    if (deleteUser.RoleID == 1)
                    {
                        var deleteStudent = aData.Students.SingleOrDefault(s => s.UserID == deleteUser.UserID);
                        if (deleteStudent != null)
                        {
                            deleteStudent.isActive = false;
                            deleteStudent.isDelete = true;
                            deleteStudent.UpdateDate = DateTime.Now;
                        }
                    }
                    else if (deleteUser.RoleID == 2)
                    {
                        var deleteInstructor = aData.Instructors.SingleOrDefault(s => s.UserID == deleteUser.UserID);
                        if (deleteInstructor != null)
                        {
                            deleteInstructor.isActive = false;
                            deleteInstructor.isDelete = true;
                            deleteInstructor.UpdateDate = DateTime.Now;
                        }
                    }
                    aData.DeletedUsers.Add(deletedUser);
                }
                aData.Users.Remove(deleteUser);
                aData.SaveChanges();
                return RedirectToAction("Users");
            }
            return View(deleteUser);
        }
        //Chức năng xóa User số lượng nhiều 
        [HttpPost]
        public ActionResult DeleteSelectedUserAccounts(List<string> usernames)
        {
            if (usernames != null && usernames.Count > 0)
            {
                foreach (string username in usernames)
                {
                    var user = aData.Users.SingleOrDefault(u => u.Username == username);
                    if (user != null)
                    {
                        DeleteUserAccount(user.UserID);
                    }
                }
                aData.SaveChanges();
            }
            return RedirectToAction("Users"); // Redirect to user list page
        }


        //Chức năng quản lý Công ty 
        //Trang quản lý Công ty 
        public ActionResult Companies()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetCompanies(DataTableParameters parameters, string sortColumn, string sortDirection)
        {
            int draw = parameters.Draw;
            int start = parameters.Start;
            int length = parameters.Length;
            string searchValue = parameters.Search.Value;

            IQueryable<Company> query = aData.Companies.Where(c => c.isDelete == false);

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(u => u.CompanyName.Contains(searchValue) ||
                                         u.Address.Contains(searchValue) ||
                                         u.Email.Contains(searchValue) ||
                                         u.Phone.Contains(searchValue));
            }
            if (!string.IsNullOrEmpty(sortColumn))
            {
                switch (sortColumn)
                {
                    case "CompanyId":
                        query = sortDirection == "asc" ? query.OrderBy(u => u.CompanyID) : query.OrderByDescending(u => u.CompanyID);
                        break;
                    case "CompanyName":
                        query = sortDirection == "asc" ? query.OrderBy(u => u.CompanyName) : query.OrderByDescending(u => u.CompanyName);
                        break;
                    case "Address":
                        query = sortDirection == "asc" ? query.OrderBy(u => u.Address) : query.OrderByDescending(u => u.Address);
                        break;
                    case "Email":
                        query = sortDirection == "asc" ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email);
                        break;
                    case "Phone":
                        query = sortDirection == "asc" ? query.OrderBy(u => u.Phone) : query.OrderByDescending(u => u.Phone);
                        break;
                    default:
                        query = query.OrderByDescending(u => u.UpdateDate);
                        break;
                }
            }

            int recordsTotal = aData.Companies.Count(c => c.isDelete == false);

            if (length == -1) // Kiểm tra nếu chọn "All"
            {
                length = recordsTotal; // Thiết lập độ dài là tổng số bản ghi
            }

            int recordsFiltered = !string.IsNullOrEmpty(searchValue) ? query.Count() : recordsTotal;

            var data = query.ToList().Skip(start).Take(length).ToList();


            // Tính toán số thứ tự
            int sequenceNumber = start + 1;

            var formattedData = data.Select(u => new
            {
                SequenceNumber = sequenceNumber++,
                CompanyID = u.CompanyID,
                Logo = u.Logo,
                CompanyName = u.CompanyName,
                Address = u.Address,
                Email = u.Email,
                Phone = u.Phone
            });

            return Json(new
            {
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = formattedData
            });
        }


        //Chức năng tạo Công ty 
        [HttpGet]
        public ActionResult CreateCompany()
        {
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCompany(Company company, HttpPostedFileBase logoCompany)
        {
            // Thêm thông báo lỗi vào ModelState cho các trường bỏ trống
            if (string.IsNullOrWhiteSpace(company.CompanyName))
            {
                ModelState.AddModelError("CompanyName", "Vui lòng nhập tên công ty.");
            }
            if (string.IsNullOrWhiteSpace(company.Email))
            {
                ModelState.AddModelError("Email", "Vui lòng nhập địa chỉ email.");
            }
            if (string.IsNullOrWhiteSpace(company.Phone))
            {
                ModelState.AddModelError("Phone", "Vui lòng nhập số điện thoại.");
            }
            if (string.IsNullOrWhiteSpace(company.Address))
            {
                ModelState.AddModelError("Address", "Vui lòng nhập địa chỉ.");
            }

            if (ModelState.IsValid)
            {
                var checkCompanyName = aData.Companies.FirstOrDefault(c => c.CompanyName == company.CompanyName);
                if (checkCompanyName == null)
                {
                    if (logoCompany != null && logoCompany.ContentLength > 0)
                    {
                        company.Logo = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + company.CompanyID + "_" + Path.GetExtension(logoCompany.FileName);
                        string fileName = Path.GetFileName(company.Logo);
                        string path = Path.Combine(Server.MapPath("~/Materials/LogoCompany"), fileName);
                        logoCompany.SaveAs(path); // Save the file to the server
                    }
                    else
                    {
                        company.Logo = "default.jpg";
                    }

                    company.isActive = true;
                    company.isDelete = false;
                    company.CreateDate = DateTime.Now;
                    company.UpdateDate = DateTime.Now;

                    aData.Companies.Add(company);
                    aData.SaveChanges();
                    return RedirectToAction("Companies");
                }
            }
            return View(company);
        }

        //Chức năng chỉnh sửa Công ty 
        [HttpGet]
        public ActionResult EditCompany(int companyID)
        {
            var existingCompany = aData.Companies.SingleOrDefault(e => e.CompanyID == companyID);
            if (existingCompany == null)
            {
                return HttpNotFound();
            }
            return PartialView(existingCompany);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCompany(Company company, int companyID)
        {
            var updateCompany = aData.Companies.SingleOrDefault(u => u.CompanyID == companyID);
            if (updateCompany != null)
            {
                updateCompany.CompanyName = company.CompanyName;
                updateCompany.Address = company.Address;
                updateCompany.Phone = company.Phone;
                updateCompany.Email = company.Email;
                updateCompany.UpdateDate = DateTime.Now;
                aData.SaveChanges();
                return RedirectToAction("Companies");
            }
            return View(updateCompany);
        }
        //Trang thông tin Công ty
        public ActionResult DetailCompany(int companyID)
        {
            var existingCompany = aData.Companies.SingleOrDefault(e => e.CompanyID == companyID);
            if (existingCompany == null)
            {
                return HttpNotFound();
            }
            return PartialView(existingCompany); 
        }
        //Chức năng xóa Công ty
        [HttpPost]
        public ActionResult DeleteCompany(int companyID)
        {
            var deleteCompany = aData.Companies.FirstOrDefault(d => d.CompanyID == companyID);
            if (deleteCompany != null)
            {
                deleteCompany.isDelete = true;
                deleteCompany.isActive = false;
                aData.SaveChanges();
                return RedirectToAction("Companies");
            }
            else
            {
                // Công ty không tồn tại, xử lý lỗi 404 hoặc thông báo lỗi khác tùy ý
                return HttpNotFound(); // Hoặc trả về một ActionResult khác để thông báo lỗi
            }
        }

        [HttpPost]
        public ActionResult DeleteSelectedCompany(List<int> companyIDs)
        {
            if (companyIDs != null && companyIDs.Any())
            {
                foreach (var companyID in companyIDs)
                {
                    DeleteCompany(companyID);
                }
                return RedirectToAction("Companies");
            }
            else
            {
                // Trả về một ActionResult khác để thông báo rằng không có công ty nào được chọn
                return RedirectToAction("Companies");
            }
        }


        //Trang quản lý Tags
        public ActionResult Tags()
        {
            var tags = aData.Tags.OrderBy(c => c.TagID).ToList();
            return View(tags);
        }
        //Chức năng tạo Tags
        [HttpGet]
        public ActionResult CreateTag()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTag(Tag tag)
        {
            if (string.IsNullOrWhiteSpace(tag.TagName))
            {
                ModelState.AddModelError("TagName", "Vui lòng nhập tên thẻ.");
            }
            if (ModelState.IsValid)
            {
                var checkTagName = aData.Tags.SingleOrDefault(c => c.TagName == tag.TagName);
                if (checkTagName != null)
                {
                    tag.CreateDate = DateTime.Now;
                    tag.UpdateDate = DateTime.Now;
                    tag.isActive = true;
                    tag.isDelete = false;
                    aData.Tags.Add(tag);
                    aData.SaveChanges();
                    return RedirectToAction("Tags");
                }
                else
                {
                    ViewBag.MessageForTag = "Thẻ đã tồn tại";
                }
            }
            return View(tag);
        }
        //Chức năng sửa Tags
        [HttpGet]
        public ActionResult EditTag(int tagID)
        {
            var checkTagID = aData.Tags.SingleOrDefault(c => c.TagID == tagID);
            if (checkTagID == null)
            {
                return HttpNotFound();
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTag(Tag tag, int tagID)
        {
            var updateTag = aData.Tags.SingleOrDefault(u => u.TagID == tagID);
            if (updateTag != null)
            {
                updateTag.TagName = tag.TagName;
                updateTag.UpdateDate = DateTime.Now;
                aData.SaveChanges();
                return RedirectToAction("EditTag", new { tagID = tag.TagID});
            }
            return View(updateTag);
        }
        //Trang thông tin Tags
        public ActionResult DetailTag(int tagID)
        {
            var tag = aData.Tags.SingleOrDefault(t => t.TagID == tagID);
            if (tag == null)
            {
                return HttpNotFound();
            }
            return View(tag);
        }
        //Chức năng xóa Tags
        public ActionResult DeleteTag(int tagID)
        {
            var deleteTag = aData.Tags.SingleOrDefault(t => t.TagID == tagID);
            if (deleteTag != null)
            {
                deleteTag.isActive = false;
                deleteTag.isDelete = true;
                aData.SaveChanges();
                return RedirectToAction("Tags");
            }
            return View(deleteTag);
        }
        //Chức năng xóa Tags đã chọn
        public ActionResult DeleteSelectedTag(List<int> tagIDs)
        {
            if (tagIDs != null && tagIDs.Count > 0)
            {
                foreach (var tagID in tagIDs)
                {
                    var existingTag = aData.Tags.SingleOrDefault(c => c.TagID == tagID);
                    if (existingTag != null)
                    {
                        existingTag.isDelete = true;
                        existingTag.isActive = false;
                    }
                }
                aData.SaveChanges();
            }
            return RedirectToAction("Tags");
        }
    }
}