using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using InternshipManagement.Models;
using Microsoft.Ajax.Utilities;
using PagedList;
using PagedList.Mvc;

namespace InternshipManagement.Controllers
{
    [RequireLogin]
    public class StudentController : Controller
    {
        InternshipManagementEntities sData = new InternshipManagementEntities();
        public ActionResult Index()
        {
            return View();
        }
        private void SendNotification(int receiverId, string message)
        {
            var notification = new Notification
            {
                ReceiverID = receiverId,
                NotificationType = "Info",
                NotificationText = message,
                NotificationDateTime = DateTime.Now,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                IsRead = false
            };

            sData.Notifications.Add(notification);
            sData.SaveChanges();
        }
        public ActionResult Navigation()
        {
            // Retrieve UserID from session
            int userID = (int)Session["UserID"];

            // Get StudentID from UserID
            int studentID = GetStudentIDFromSession(userID);

            if (studentID != 0)
            {
                var internshipInfo = sData.InternshipInformations.FirstOrDefault(i => i.StudentID == studentID);

                if (internshipInfo != null)
                {
                    return RedirectToAction("DetailProject", new { studentID = studentID });
                }
                else
                {
                    return RedirectToAction("SearchProject");
                }
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }
        private int GetStudentIDFromSession(int userID)
        {
            var student = sData.Students.FirstOrDefault(s => s.UserID == userID);
            if (student != null)
            {
                return student.StudentID;
            }
            return 0;
        }


        public ActionResult headerMenu(int uid)
        {
            var existingUser = sData.Users.SingleOrDefault(e => e.UserID == uid);
            return PartialView("_studentMenu", existingUser);
        }
        //Trang thông tin Sinh viên
        public ActionResult StudentPage(string studentUserName)
        {
            var existingUser = sData.Users.SingleOrDefault(u => u.Username == studentUserName);
            var existingStudent = sData.Students.SingleOrDefault(e => e.UserID == (int)Session["UserID"]);
            var checkStudent = sData.Students.SingleOrDefault(c => c.UserID == existingUser.UserID).Equals(existingStudent.UserID);
            if (checkStudent == true)
            {
                if (existingStudent == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    return View(existingStudent);
                }
            }
            else
            {
                return View(existingStudent.CV);
            }
        }
        //Chức năng Chỉnh sửa thông tin Sinh viên
        [HttpGet]
        public ActionResult EditStudentPage(string studentUserName)
        {
            var existingUser = sData.Users.SingleOrDefault(u => u.Username == studentUserName);
            var existingStudent = sData.Students.SingleOrDefault(e => e.UserID == (int)Session["UserID"]);
            var checkStudent = sData.Students.SingleOrDefault(c => c.UserID == existingUser.UserID).Equals(existingStudent.UserID);
            if (checkStudent == true)
            {
                if (existingStudent == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    return View(existingStudent);
                }
            }
            else
            {
                return View(existingStudent.CV);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditStudentPage(string studentUserName, HttpPostedFileBase avatarFile, Student student)
        {
            var existingUser = sData.Users.SingleOrDefault(u => u.Username == studentUserName);
            var existingStudent = sData.Students.SingleOrDefault(e => e.UserID == (int)Session["UserID"]);
            var updateStudent = sData.Students.SingleOrDefault(u => u.StudentID == existingStudent.StudentID);
            if (updateStudent != null)
            {
                updateStudent.FirstName = student.FirstName;
                updateStudent.LastName = student.LastName;
                updateStudent.DateOfBirth = student.DateOfBirth;
                updateStudent.Gender = student.Gender;
                updateStudent.Class = student.Class;
                updateStudent.Email = student.Email;
                updateStudent.Phone = student.Phone;
                updateStudent.Address = student.Address;
                updateStudent.UpdateDate = DateTime.Now;
                
                // Process the avatar file
                if (avatarFile != null && avatarFile.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(avatarFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/Materials/Avatars"), fileName);
                    avatarFile.SaveAs(path); // Save the file to the server
                    updateStudent.Avatar = fileName; // Save the file name to the database

                    //Cập nhật Avatar cho User
                    var updateUser = sData.Users.SingleOrDefault(u => u.UserID == student.UserID);
                    if (updateUser != null)
                    {
                        updateUser.Avatar = updateStudent.Avatar;
                    }
                }

                sData.SaveChanges();
                return RedirectToAction("StudentPage");
            }
            return View(updateStudent);
        }
        //Chức năng lưu dự án yêu thích
        public ActionResult SaveFavoriteProject(int studentID, int projectID, FavoriteProject fvProject, Project project)
        {
            var existingProject = sData.Projects.SingleOrDefault(e => e.ProjectID == projectID);
            var existingStudent = sData.Students.SingleOrDefault(e => e.StudentID == studentID);

            if (existingProject == null || existingStudent == null)
            {
                return HttpNotFound(); // Nếu dự án hoặc sinh viên không tồn tại, trả về lỗi 404
            }
            else
            {
                fvProject.StudentID = studentID;
                fvProject.ProjectID = projectID;
                sData.FavoriteProjects.Add(fvProject); // Thêm vào danh sách dự án yêu thích
                project.LikesCount++;
                sData.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
            }
            return View(); // Trả về view hoặc có thể chuyển hướng tùy thuộc vào yêu cầu của bạn
        }
        //Trang quản lý dự án yêu thích
        public ActionResult FavoritePorjects(int studentID)
        {
            var favoriteProjects = sData.FavoriteProjects.Where(f => f.StudentID == studentID).ToList();
            return View(favoriteProjects);
        }
        //Chức năng xóa dự án yêu thích
        public ActionResult DeleteFavoriteProject(int favoriteID)
        {
            var deleteFavoriteProject = sData.FavoriteProjects.SingleOrDefault(d => d.FavoriteID == favoriteID);
            if (deleteFavoriteProject == null)
            {
                return HttpNotFound();
            }
            else
            {
                sData.FavoriteProjects.Remove(deleteFavoriteProject); 
                sData.SaveChanges();
                return RedirectToAction("FavoriteProjects");
            }
        }
        //Chức năng xóa nhiều dự án
        public ActionResult DeleteSelectedFavorite(List<int> favoriteIDs)
        {
            if (favoriteIDs != null && favoriteIDs.Count > 0)
            {
                foreach (var farvoriteID in favoriteIDs)
                {
                    var existingFavoriteProject = sData.FavoriteProjects.SingleOrDefault(c => c.FavoriteID== farvoriteID);
                    if (existingFavoriteProject != null)
                    {
                        sData.FavoriteProjects.Remove(existingFavoriteProject);
                    }
                }
                sData.SaveChanges();
            }
            return RedirectToAction("FavoriteProjects");
        }
        //Chức năng apply vào dự án 
        public ActionResult ApplyProject(int projectID)
        {
            try
            {
                int userID = Convert.ToInt32(Session["UserID"]);
                int studentID = GetStudentIDFromSession(userID);

                if (IsStudentInAnyProject(studentID))
                {
                    ModelState.AddModelError("", "Sinh viên đã tham gia một dự án khác.");
                    return View();
                }

                var student = sData.Students.Find(studentID);
                var project = sData.Projects.Find(projectID);

                if (student == null)
                {
                    ModelState.AddModelError("", "Sinh viên không tồn tại.");
                    return View();
                }

                if (project == null)
                {
                    ModelState.AddModelError("", "Dự án không tồn tại.");
                    return View();
                }

                if (CheckMemberInProject(projectID))
                {
                    ModelState.AddModelError("", "Dự án đã đủ thành viên.");
                    return View();
                }

                var existingRecord = sData.InternshipQueues.FirstOrDefault(x => x.StudentID == studentID && x.ProjectID == projectID);

                if (existingRecord != null)
                {
                    ModelState.AddModelError("", "Dữ liệu đã tồn tại trong hàng đợi");
                    return View("ApplyProject");
                }

                InternshipQueue newRecord = new InternshipQueue
                {
                    StudentID = studentID,
                    ProjectID = projectID
                };

                sData.InternshipQueues.Add(newRecord);
                sData.SaveChanges();

                // Send notification
                var studentName = $"{student.FirstName} {student.LastName}";
                var instructor = sData.Users.Find(project.InstructorID);
                var instructorName = $"{instructor.FirstName} {instructor.LastName}";

                SendNotification(studentID, $"Bạn đã nộp đơn xin tham gia dự án {project.ProjectName}.");
                SendNotification(project.InstructorID.Value, $"Sinh viên {studentName} đã yêu cầu tham gia vào dự án {project.ProjectName}.");

                return RedirectToAction("SearchProject");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
                return RedirectToAction("SearchProject");
            }
        }

        //Trang tìm kiếm dự án đành cho Sinh viên
        [HttpGet]
        public ActionResult SearchProject(string searchString, int? page, int? pageSize, string sortBy, List<int> specializations)
        {
            // Khởi tạo đối tượng context
            InternshipManagementEntities context = new InternshipManagementEntities();

            // Lấy danh sách dự án từ cơ sở dữ liệu và áp dụng tìm kiếm nếu có
            var projectsQuery = string.IsNullOrEmpty(searchString)
                ? context.Projects
                : context.Projects.Where(p => p.ProjectName.Contains(searchString));

            // Filter by selected specializations
            if (specializations != null && specializations.Any())
            {
                projectsQuery = projectsQuery.Where(p => p.ProjectSpecializations.Any(ps => specializations.Contains(ps.SpecializationID.Value)));
            }
            // Sorting projects based on sortBy parameter
            switch (sortBy)
            {
                case "mostLiked":
                    projectsQuery = projectsQuery.OrderByDescending(p => p.LikesCount);
                    break;
                case "newest":
                default:
                    // Default sorting by newest projects
                    projectsQuery = projectsQuery.OrderByDescending(p => p.CreateDate);
                    break;
            }

            // Lấy danh sách dự án đã được lọc và sắp xếp
            var projects = projectsQuery.ToList();

            // Chuyển đổi dữ liệu dự án thành một danh sách ViewModel để truyền đến view
            List<ProjectViewModel> projectViewModels = new List<ProjectViewModel>();

            foreach (var project in projects)
            {
                // Tạo một đối tượng ViewModel từ dữ liệu dự án
                ProjectViewModel viewModel = new ProjectViewModel
                {
                    ProjectID = project.ProjectID,
                    Logo = project.Company.Logo,
                    CompanyName = project.Company.CompanyName,
                    InstructorName = project.Instructor.LastName + " " + project.Instructor.FirstName,
                    ProjectName = project.ProjectName,
                    Description = project.Description,
                    LikesCount = project.LikesCount.ToString(),
                    Tags = context.ProjectTags.Where(pt => pt.ProjectID == project.ProjectID)
                                               .Select(pt => pt.Tag.TagName)
                                               .ToList()
                };

                // Thêm ViewModel vào danh sách
                projectViewModels.Add(viewModel);
            }

            // Thiết lập số lượng dự án trên mỗi trang mặc định là 10
            int pageSizeValue = pageSize ?? 10;

            ViewBag.Specializations = context.Specializations.ToList();

            // Truyền chuỗi tìm kiếm vào ViewBag để giữ lại giá trị tìm kiếm khi chuyển trang
            ViewBag.CurrentFilter = searchString;

            // Lưu số lượng dự án trên mỗi trang vào ViewBag để hiển thị trong dropdown
            ViewBag.PageSize = pageSizeValue;

            // Trả về view và truyền danh sách ViewModel của trang hiện tại và thông tin phân trang cho view
            return View(projectViewModels.ToPagedList(page ?? 1, pageSizeValue));
        }
        //Trang chi tiết dự án
        public ActionResult DetailProject(int studentID)
        {
            var context = new InternshipManagementEntities();

            // Lấy thông tin thực tập của sinh viên
            var internship = context.InternshipInformations
                .Include("Project")
                .Include("Project.Company")
                .Include("Project.Instructor")
                .Include("Project.Tasks")
                .Include("Project.InternshipInformations.Student")
                .SingleOrDefault(p => p.StudentID == studentID && p.isActive == true);

            if (internship == null)
            {
                return RedirectToAction("SearchProject"); // Nếu không tìm thấy, chuyển hướng tới trang khác
            }

            // Tạo ViewModel để truyền dữ liệu tới view
            var projectDetailViewModel = new ProjectDetailViewModel
            {
                ProjectID = internship.ProjectID.Value,
                ProjectName = internship.Project.ProjectName,
                CompanyName = internship.Project.Company.CompanyName,
                CompanyLogo = internship.Project.Company.Logo,
                InstructorName = internship.Project.Instructor.LastName + " " + internship.Project.Instructor.FirstName,
                Members = internship.Project.InternshipInformations
                                    .Where(i => i.isActive == true)
                                    .Select(i => new ProjectMemberViewModel
                                    {
                                        StudentID = i.Student.StudentID,
                                        StudentName = i.Student.LastName + " " + i.Student.FirstName,
                                        Avatar = i.Student.Avatar
                                    }).ToList(),
                Tasks = internship.Project.Tasks
                                     .Where(t => t.StudentID == studentID && t.isDelete == false)
                                     .Select(t => new TaskViewModel
                                     {
                                         TaskID = t.TaskID,
                                         Description = t.TaskDescription,
                                         Status = t.Status,
                                         StartDate = t.StartDate,
                                         EndDate = t.EndDate
                                     }).ToList()
            };

            return View(projectDetailViewModel);
        }
        //Trang chi tiết dự án 
        public ActionResult singleProject(int ProjectID)
        {
            var project = sData.Projects.SingleOrDefault(p => p.ProjectID == ProjectID);
            return View(project);
        }
        //Kiểm tra Sinh viên đã tham gia dự án chưa? 
        public ActionResult CheckProject(int studentID)
        {
            var internship = sData.InternshipInformations.SingleOrDefault(p => p.StudentID == studentID);
            if (internship != null)
            {
                ViewBag.Message = "Sinh viên chỉ tham gia 1 dự án";
            }
            return View();
        }
        //Kiểm tra dự án đã đủ thành viên chưa? 
        public bool CheckMemberInProject(int projectID)
        {
            var project = sData.Projects.SingleOrDefault(p => p.ProjectID == projectID);
            if (project != null)
            {
                if (project.StudentsCount == project.MaxStudents)
                {
                    ViewBag.Message = "Dự án đã đủ thành viên. Không thể tham gia dự án!";
                    return true; // Đã đủ thành viên
                }
                else
                {
                    ViewBag.Message = "Dự án vẫn còn chỗ trống. Bạn có thể tham gia dự án!";
                    return false; // Chưa đủ thành viên
                }
            }
            else
            {
                ViewBag.Message = "Không tìm thấy dự án!";
                return false; // Dự án không tồn tại
            }
        }
        //Kiểm tra sinh viên có trong dự án nào không? 
        private bool IsStudentInAnyProject(int studentID)
        {
            var internship = sData.InternshipInformations.SingleOrDefault(p => p.StudentID == studentID);
            return internship != null;
        }
        //Chức năng cập nhật trạng thái cho dự án
        [HttpPost]
        public ActionResult UpdateStatus(int taskId, string newStatus)
        {
            var context = new InternshipManagementEntities();
            var task = context.Tasks.FirstOrDefault(t => t.TaskID == taskId);
            if (task != null)
            {
                task.Status = newStatus;
                context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

    }
}