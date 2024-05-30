using System;
using System.Collections.Generic;
using System.Configuration.Internal;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Antlr.Runtime.Tree;
using InternshipManagement.Models;
using Newtonsoft.Json;

namespace InternshipManagement.Controllers
{
    [RequireLogin]
    public class InstructorController : Controller
    {
        InternshipManagementEntities iData = new InternshipManagementEntities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Navigation()
        {
            int instructorID = GetInstructorIDFromSession();

            if (instructorID != 0)
            {
                var project = iData.Projects.FirstOrDefault(p => p.InstructorID == instructorID);

                if (project != null)
                {
                    return RedirectToAction("Projects");
                }
                else
                {
                    return RedirectToAction("CreateProject");
                }
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        private int GetInstructorIDFromSession()
        {
            if (Session["UserID"] != null)
            {
                int userID = Convert.ToInt32(Session["UserID"]);
                var instructor = iData.Instructors.FirstOrDefault(i => i.UserID == userID);
                if (instructor != null)
                {
                    return instructor.InstructorID;
                }

            }
            return 0;
        }
        public ActionResult headerMenu(int uid)
        {
            var existingUser = iData.Users.SingleOrDefault(e => e.UserID == uid);
            return PartialView("_instructorMenu", existingUser);
        }
        //Trang thông tin Giảng viên
        public ActionResult InstructorPage(string instructorUserName)
        {
            var existingUser = iData.Users.SingleOrDefault(u => u.Username == instructorUserName);
            var existingInstructor = iData.Instructors.SingleOrDefault(e => e.UserID == (int)Session["UserID"]);
            var checkInstructor = iData.Students.SingleOrDefault(c => c.UserID == existingUser.UserID).Equals(existingInstructor.UserID);
            if (checkInstructor == true)
            {
                if (existingInstructor == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    return View(existingInstructor);
                }
            }
            else
            {
                return HttpNotFound();
            }
        }
        //Chỉnh sửa thông tin Giảng viên
        [HttpGet]
        public ActionResult EditInstructorPage(string instructorUserName)
        {
            var existingUser = iData.Users.SingleOrDefault(u => u.Username == instructorUserName);
            var existingInstructor = iData.Instructors.SingleOrDefault(e => e.UserID == (int)Session["UserID"]);
            var checkInstructor = iData.Students.SingleOrDefault(c => c.UserID == existingUser.UserID).Equals(existingInstructor.UserID);
            if (checkInstructor == true)
            {
                if (existingInstructor == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    return View(existingInstructor);
                }
            }
            else
            {
                return HttpNotFound();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditInstructorPage(string instructorUserName, HttpPostedFileBase avatarFile, Instructor instructor)
        {
            var existingUser = iData.Users.SingleOrDefault(u => u.Username == instructorUserName);
            var existingInstructor = iData.Instructors.SingleOrDefault(e => e.UserID == (int)Session["UserID"]);
            var updateInstructor = iData.Instructors.SingleOrDefault(u => u.InstructorID == existingInstructor.InstructorID);
            if (updateInstructor != null)
            {
                updateInstructor.FirstName = instructor.FirstName;
                updateInstructor.LastName = instructor.LastName;
                updateInstructor.DateOfBirth = instructor.DateOfBirth;
                updateInstructor.Gender = instructor.Gender;
                updateInstructor.Email = instructor.Email;
                updateInstructor.Phone = instructor.Phone;
                updateInstructor.UpdateDate = DateTime.Now;

                // Process the avatar file
                if (avatarFile != null && avatarFile.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(avatarFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/Materials/Avatars"), fileName);
                    avatarFile.SaveAs(path); // Save the file to the server
                    updateInstructor.Avatar = fileName; // Save the file name to the database

                    //Cập nhật Avatar cho User
                    var updateUser = iData.Users.SingleOrDefault(u => u.UserID == instructor.UserID);
                    if (updateUser != null)
                    {
                        updateUser.Avatar = updateInstructor.Avatar;
                    }
                }

                iData.SaveChanges();
                return RedirectToAction("InstructorPage", new { instructorID = instructor.InstructorID });
            }
            return View(updateInstructor);
        }
        //Chức năng quản lý dự án của Giảng viên
        public ActionResult Projects()
        {
            int instructorID = GetInstructorIDFromSession();

            if (instructorID == 0)
            {
                return Json(new { error = "Session expired" });
            }

            var projectsDetails = iData.Projects
                .Where(p => p.InstructorID == instructorID)
                .Select(p => new ProjectDetails
                {
                    ProjectID = p.ProjectID,
                    ProjectName = p.ProjectName,
                    StudentsCount = iData.InternshipInformations.Count(ii => ii.ProjectID == p.ProjectID),
                    BackgroundPicture = p.BackgroundPicture,
                    Description = p.Description,
                    MaxStudents = p.MaxStudents ?? 0,
                    LikesCount = p.LikesCount ?? 0,
                    CompanyID = p.CompanyID ?? 0,
                    CompanyName = p.Company.CompanyName,
                    InstructorID = p.InstructorID ?? 0,
                    IsActive = p.isActive ?? false,
                    IsDelete = p.isDelete ?? false,
                    Members = p.InternshipInformations.Select(ii => ii.Student).ToList(),
                    Tasks = p.Tasks.ToList(),
                    QueueStudents = iData.InternshipQueues
                        .Where(q => q.ProjectID == p.ProjectID && q.StudentID.HasValue)
                        .Select(q => q.Student)
                        .ToList(),
                    Personalize = iData.Personalizes.FirstOrDefault(pe => pe.ProjectID == p.ProjectID)
                })
                .ToList();

            // Sắp xếp danh sách dự án: dự án đã ghim lên trên đầu, sau đó sắp xếp theo tên dự án
            projectsDetails = projectsDetails
                .OrderByDescending(p => p.Personalize?.Pinned ?? false)
                .ThenBy(p => p.ProjectName)
                .ToList();

            if (projectsDetails == null || !projectsDetails.Any())
            {
                return View("EmptyProjectsView");
            }

            return View(projectsDetails);
        }

        [HttpPost]
        public ActionResult PinProject(int projectId)
        {
            var project = iData.Personalizes.FirstOrDefault(p => p.ProjectID == projectId);
            if (project == null)
            {
                // Tạo mới bản ghi Personalize nếu chưa có
                project = new Personalize
                {
                    ProjectID = projectId,
                    Pinned = true // Mặc định là ghim
                };
                iData.Personalizes.Add(project);
            }
            else
            {
                project.Pinned = true; // Cập nhật trạng thái ghim
            }
            iData.SaveChanges();

            return RedirectToAction("Projects");
        }

        [HttpPost]
        public ActionResult UnpinProject(int projectId)
        {
            var project = iData.Personalizes.FirstOrDefault(p => p.ProjectID == projectId);
            if (project == null)
            {
                // Tạo mới bản ghi Personalize nếu chưa có
                project = new Personalize
                {
                    ProjectID = projectId,
                    Pinned = false // Mặc định là không ghim
                };
                iData.Personalizes.Add(project);
            }
            else
            {
                project.Pinned = false; // Cập nhật trạng thái bỏ ghim
            }
            iData.SaveChanges();

            return RedirectToAction("Projects");
        }

        [HttpPost]
        public ActionResult ChangeProjectColor(int projectId, string color)
        {
            var project = iData.Personalizes.FirstOrDefault(p => p.ProjectID == projectId);
            if (project == null)
            {
                // Tạo mới bản ghi Personalize nếu chưa có
                project = new Personalize
                {
                    ProjectID = projectId,
                    Color = color
                };
                iData.Personalizes.Add(project);
            }
            else
            {
                project.Color = color; // Cập nhật màu nền
            }
            iData.SaveChanges();

            return RedirectToAction("Projects");
        }

        //Chức năng tạo Dự án (Project)
        [HttpGet]
        public ActionResult CreateProject()
        {
            ViewBag.Companies = new SelectList(iData.Companies.ToList().OrderBy(r => r.CompanyID), "CompanyID", "CompanyName");
            ViewBag.Tags = new MultiSelectList(iData.Tags.ToList().OrderBy(t => t.TagID), "TagID", "TagName"); // Tạo MultiSelectList của các tag
            ViewBag.Specializations = new MultiSelectList(iData.Specializations.ToList().OrderBy(s => s.SpecializationID), "SpecializationID", "SpecializationName");
            return PartialView();
        }
        [HttpPost]
        public ActionResult CreateProject(Project project, HttpPostedFileBase backgroundPicture, Instructor instructor, string[] Tags, string[] Specializations)
        {
            // Thêm thông báo lỗi vào ModelState cho các trường bỏ trống
            if (string.IsNullOrWhiteSpace(project.ProjectName))
            {
                ModelState.AddModelError("ProjectName", "Vui lòng nhập tên dự án.");
            }
            if (string.IsNullOrWhiteSpace(project.Description))
            {
                ModelState.AddModelError("Description", "Vui lòng nhập mô tả dự án.");
            }
            if (project.MaxStudents == null)
            {
                ModelState.AddModelError("MaxStudents", "Vui lòng nhập số lượng sinh viên tối đa sinh viên có thể tham gia.");
            }

            //Các SelectList cho Công ty, Tags, Chuyên ngành
            ViewBag.Companies = new SelectList(iData.Companies.ToList().OrderBy(r => r.CompanyID), "CompanyID", "CompanyName");
            ViewBag.Tags = new MultiSelectList(iData.Tags.ToList().OrderBy(t => t.TagID), "TagID", "TagName"); // Tạo MultiSelectList của các tag
            ViewBag.Specializations = new MultiSelectList(iData.Specializations.ToList().OrderBy(s => s.SpecializationID), "SpecializationID", "SpecializationName");
            int instructorId = (int)Session["UserID"];
            var instructorCompany = iData.Instructors.FirstOrDefault(i => i.UserID == instructorId);

            if (ModelState.IsValid)
            {

                var checkCompanyName = iData.Projects.FirstOrDefault(c => c.ProjectName == project.ProjectName);
                if (checkCompanyName == null)
                {
                    project.InstructorID = instructorCompany.InstructorID; // Gán InstructorID cho dự án
                    project.CompanyID = instructorCompany.CompanyID;
                    project.isActive = true;
                    project.isDelete = false;
                    project.CreateDate = DateTime.Now;
                    project.UpdateDate = DateTime.Now;
                    project.LikesCount = 0;

                    // Process the avatar file
                    if (backgroundPicture != null && backgroundPicture.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(backgroundPicture.FileName);
                        string path = Path.Combine(Server.MapPath("~/Materials/BackgroundPicture"), fileName);
                        backgroundPicture.SaveAs(path); // Save the file to the server
                        project.BackgroundPicture = fileName; // Save the file name to the database
                    }

                    // Add specializations to ProjectSpecialization
                    if (Specializations != null)
                    {
                        foreach (var specializationId in Specializations)
                        {
                            var projectSpecialization = new ProjectSpecialization
                            {
                                ProjectID = project.ProjectID,
                                SpecializationID = int.Parse(specializationId)
                            };
                            iData.ProjectSpecializations.Add(projectSpecialization);
                        }
                    }

                    iData.Projects.Add(project);
                    iData.SaveChanges();
                    // Get the tag names corresponding to the selected tag IDs
                    var selectedTagNames = iData.Tags.Where(t => Tags.Contains(t.TagID.ToString())).Select(t => t.TagName).ToArray();
                    return AddTag(project.ProjectID, selectedTagNames);
                }
            }
            return View(project);
        }


        public ActionResult SearchTag(string query)
        {
            var tags = iData.Tags
                .Where(t => t.TagName.Contains(query))
                .Select(t => new { id = t.TagID.ToString(), text = t.TagName })
                .ToList();
            return Json(tags, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddTag(int projectID, string[] Tags)
        {
            if (Tags != null && Tags.Any())
            {
                foreach (var tagName in Tags)
                {
                    // Tìm thẻ trong CSDL dựa trên tên
                    var existingTag = iData.Tags.FirstOrDefault(t => t.TagName == tagName);

                    // Nếu thẻ không tồn tại, tạo mới
                    if (existingTag == null)
                    {
                        existingTag = new Tag
                        {
                            TagName = tagName,
                            isActive = true,
                            isDelete = false,
                            CreateDate = DateTime.Now,
                            UpdateDate = DateTime.Now
                        };

                        iData.Tags.Add(existingTag);
                        iData.SaveChanges(); // Lưu thay đổi để lấy TagID
                    }

                    // Kiểm tra xem thẻ đã được liên kết với dự án chưa
                    var checkTag = iData.ProjectTags.FirstOrDefault(pt => pt.TagID == existingTag.TagID && pt.ProjectID == projectID);

                    // Nếu thẻ chưa được liên kết với dự án, thêm vào
                    if (checkTag == null)
                    {
                        var newProjectTag = new ProjectTag
                        {
                            ProjectID = projectID,
                            TagID = existingTag.TagID
                        };

                        iData.ProjectTags.Add(newProjectTag);
                    }
                    else
                    {
                        // Thẻ đã được liên kết với dự án
                        ViewBag.Message = "Thẻ đã tồn tại.";
                    }
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                iData.SaveChanges();
            }

            // Chuyển hướng đến hành động Projects sau khi thêm thẻ
            return RedirectToAction("Projects");
        }

        public List<Tag> Tags { get; set; }
        //Chức năng chỉnh sửa Dự án (Project)
        [HttpGet]
        public ActionResult EditProjects(int projectID)
        {
            var existingProject = iData.Projects.SingleOrDefault(p => p.ProjectID == projectID);
            if (existingProject == null)
            {
                return HttpNotFound();
            }
            return View(existingProject);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProjects(int projectID, Project project)
        {
            var updateProject = iData.Projects.SingleOrDefault(u => u.ProjectID == projectID);
            if (updateProject != null)
            {
                updateProject.ProjectName = project.ProjectName;
                updateProject.Description = project.Description;
                updateProject.MaxStudents = project.MaxStudents;
                updateProject.UpdateDate = DateTime.Now;
                iData.SaveChanges();
                return RedirectToAction("EditProjects", new { projectID = project.ProjectID });
            }
            return View(updateProject);
        }
        //Trang thông tin Project
        public ActionResult ProjectDetails(int id)
        {
            // Get the project with the given id
            var project = iData.Projects.SingleOrDefault(p => p.ProjectID == id);
            var instructor = iData.Instructors.SingleOrDefault(i => i.InstructorID == project.InstructorID);

            // If the project doesn't exist, return a 404 error
            if (project == null)
            {
                return HttpNotFound();
            }

            // Create a ProjectDetails object
            var projectDetails = new ProjectDetails
            {
                CreateDate = project.CreateDate ?? DateTime.Now,
                UpdateDate = project.UpdateDate ?? DateTime.Now,
                ProjectID = project.ProjectID,
                ProjectName = project.ProjectName,
                StudentsCount = iData.InternshipInformations.Count(ii => ii.ProjectID == project.ProjectID),
                InstructorFirstName = instructor?.FirstName,
                InstructorLastName = instructor?.LastName,
                BackgroundPicture = project.BackgroundPicture,
                Description = project.Description,
                MaxStudents = project.MaxStudents ?? 0,
                LikesCount = project.LikesCount ?? 0,
                CompanyID = project.CompanyID ?? 0,
                CompanyName = project.Company.CompanyName,
                InstructorID = project.InstructorID ?? 0,
                IsActive = project.isActive ?? false,
                IsDelete = project.isDelete ?? false,
                Members = project.InternshipInformations.Select(ii => ii.Student).ToList(),
                Tasks = project.Tasks.ToList(),
                QueueStudents = iData.InternshipQueues
                                    .Where(q => q.ProjectID == project.ProjectID && q.StudentID.HasValue)
                                    .Select(q => q.Student)
                                    .ToList()
            };

            // Pass the success message to the view if it exists
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            // Pass the ProjectDetails object to the view
            return View(projectDetails);
        }

        //Chức năng xóa Project
        public ActionResult DeleteProject(int projectID, Project project)
        {
            var deleteProject = iData.Projects.SingleOrDefault(d => d.ProjectID == projectID);
            if (deleteProject != null)
            {
                deleteProject.isDelete = true;
                deleteProject.isActive = false;
                iData.SaveChanges();
                return RedirectToAction("Projects");
            }
            return View(deleteProject);
        }
        //Xóa nhiều project cùng lúc 
        public ActionResult DeleteSelectedProject(List<int> projectIDs)
        {
            if (projectIDs != null && projectIDs.Count > 0)
            {
                foreach (var projectID in projectIDs)
                {
                    var existingProject = iData.Projects.SingleOrDefault(c => c.ProjectID == projectID);
                    if (existingProject != null)
                    {
                        existingProject.isDelete = true;
                        existingProject.isActive = false;
                    }
                }
                iData.SaveChanges();
            }
            return RedirectToAction("Projects");
        }
        //Chức năng duyệt sinh viên tham gia vào dự án 
        [HttpPost]
        public ActionResult ApproveStudent(int studentId, int projectId)
        {
            // Find the student in the queue
            var queueEntry = iData.InternshipQueues.SingleOrDefault(q => q.StudentID == studentId && q.ProjectID == projectId);

            if (queueEntry == null)
            {
                return HttpNotFound();
            }

            // Remove student from the queue
            iData.InternshipQueues.Remove(queueEntry);

            var project = iData.Projects.SingleOrDefault(p => p.ProjectID == projectId);

            if (project.StudentsCount == project.MaxStudents)
            {
                ViewBag.Message = "Số lượng sinh viên đã đủ cho dự án!";
                return View();
            }
            else
            {
                project.StudentsCount++;

                var newInternship = new InternshipInformation();
                newInternship.StudentID = studentId;
                newInternship.ProjectID = projectId;
                newInternship.CompanyID = project.CompanyID;
                newInternship.InstructorID = project.InstructorID;
                newInternship.StartDate = null;
                newInternship.EndDate = null;
                newInternship.CreateDate = DateTime.Now;
                newInternship.UpdateDate = DateTime.Now;
                newInternship.Description = null;
                newInternship.isActive = true;
                newInternship.isDelete = false;
                iData.InternshipInformations.Add(newInternship);
            }
            // Save changes
            iData.SaveChanges();

            // Redirect back to the project details
            return RedirectToAction("ProjectDetails", new { id = projectId });
        }
        //Chức năng từ chối sinh viên tham gia vào dự án 
        public ActionResult RejectStudent(int studentId, int projectId)
        {
            var queueEntry = iData.InternshipQueues.SingleOrDefault(q => q.StudentID == studentId && q.ProjectID == projectId);

            if (queueEntry == null)
            {
                return HttpNotFound();
            }
            iData.InternshipQueues.Remove(queueEntry);
            iData.SaveChanges();

            return RedirectToAction("ProjectDetails", new { id = projectId });
        }
        //Chức năng chỉnh sửa thông tin sinh viên trong dự án
        [HttpGet]
        public ActionResult EditInternship(int internshipID)
        {
            var existingInternship = iData.InternshipInformations.SingleOrDefault(e => e.InternshipID == internshipID);
            if (internshipID == null)
            {
                return HttpNotFound();
            }
            return View(existingInternship);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditInternship(int internshipID, InternshipInformation internship)
        {
            var updateInternship = iData.InternshipInformations.SingleOrDefault(u => u.InternshipID == internshipID);
            if (updateInternship != null)
            {
                updateInternship.Description = internship.Description;
                updateInternship.StartDate = internship.StartDate;
                updateInternship.EndDate = internship.EndDate;
                iData.SaveChanges();
                return RedirectToAction("StudentManagement", new { projectID = internship.ProjectID });
            }
            return View();
        }
        public ActionResult Tasks(int projectID)
        {
            var tasksInProject = iData.Tasks.FirstOrDefault(t => t.ProjectID == projectID);
            return View(tasksInProject);
        }
        //Chức năng tạo Task cho sinh viên
        [HttpGet]
        public ActionResult CreateTask(int projectID)
        {
            // Lấy danh sách sinh viên có cùng ProjectID từ bảng InternshipInformations
            var studentsWithProjectID = iData.InternshipInformations
                                            .Where(c => c.ProjectID == projectID)
                                            .OrderBy(s => s.StudentID)
                                            .Select(info => new
                                            {
                                                StudentId = info.StudentID,
                                                FullName = info.Student.FirstName + " " + info.Student.LastName
                                            })
                                            .ToList();

            if (studentsWithProjectID != null)
            {
                // Tạo SelectList từ danh sách sinh viên
                ViewBag.Students = new SelectList(studentsWithProjectID, "StudentId", "FullName");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTask(InternshipManagement.Models.Task task, int projectID)
        {
            // Lấy danh sách sinh viên có cùng ProjectID từ bảng InternshipInformations
            var studentsWithProjectID = iData.InternshipInformations
                                            .Where(c => c.ProjectID == projectID)
                                            .OrderBy(s => s.StudentID)
                                            .Select(info => new
                                            {
                                                StudentId = info.StudentID,
                                                FullName = info.Student.FirstName + " " + info.Student.LastName
                                            })
                                            .ToList();

            if (studentsWithProjectID != null)
            {
                // Tạo SelectList từ danh sách sinh viên
                ViewBag.Students = new SelectList(studentsWithProjectID, "StudentId", "FullName");
            }

            // Tạo một đối tượng Task mới
            var newTask = new InternshipManagement.Models.Task
            {
                TaskDescription = task.TaskDescription,
                CompletionDate = null,
                Status = task.Status,
                ProjectID = projectID,
                StudentID = task.StudentID,
                InstructorID = task.InstructorID, // Giả sử sử dụng InstructorID từ dự án
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                isDelete = false, // Mặc định không được xóa
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now
            };

            // Thêm đối tượng Task mới vào bảng Tasks
            iData.Tasks.Add(newTask);

            // Lưu các thay đổi vào cơ sở dữ liệu
            iData.SaveChanges();

            // Đặt thông báo thành công vào TempData
            TempData["SuccessMessage"] = $"Tạo task thành công.";


            // Chuyển hướng người dùng đến trang ProjectDetails sau khi thêm nhiệm vụ thành công
            return RedirectToAction("ProjectDetails", new { id = projectID });
        }

        //Chức năng chỉnh sửa Task
        [HttpGet]
        public ActionResult EditTask(int taskID)
        {
            var task = iData.Tasks.SingleOrDefault(t => t.TaskID == taskID);
            if (task == null)
            {
                return HttpNotFound();
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTask(int taskID, InternshipManagement.Models.Task task)
        {
            var updateTask = iData.Tasks.SingleOrDefault(u => u.TaskID == taskID);
            if (updateTask != null)
            {
                updateTask.TaskDescription = task.TaskDescription;
                updateTask.CompletionDate = task.CompletionDate;
                updateTask.Status = task.Status;
                updateTask.StudentID = task.StudentID;
                updateTask.UpdateDate = DateTime.Now;
                iData.SaveChanges();
                return RedirectToAction("EditTask", new { taskID = task.TaskID });
            }
            return View(updateTask);
        }
        //Trang thông tin Task
        public ActionResult DetailTask(int taskID)
        {
            var detailTask = iData.Tasks.SingleOrDefault(d => d.TaskID == taskID);
            if (detailTask == null)
            {
                return HttpNotFound();
            }
            return View(detailTask);
        }
        [HttpPost]
        public ActionResult UpdateTaskStatus(int taskID, string newStatus)
        {
            // Tìm task có ID tương ứng trong cơ sở dữ liệu
            var taskToUpdate = iData.Tasks.SingleOrDefault(t => t.TaskID == taskID);

            // Kiểm tra nếu task tồn tại
            if (taskToUpdate != null)
            {
                // Cập nhật trạng thái mới cho task
                taskToUpdate.Status = newStatus;

                // Lưu các thay đổi vào cơ sở dữ liệu
                iData.SaveChanges();

                // Tính toán lại tiến độ dự án
                if (iData.Projects.Any())
                {
                    int completedTasksCount = iData.Tasks.Count(t => t.ProjectID == taskToUpdate.ProjectID && t.Status == "Completed");
                    int totalTasksCount = iData.Tasks.Count(t => t.ProjectID == taskToUpdate.ProjectID);
                    int percentage = totalTasksCount > 0 ? (completedTasksCount * 100 / totalTasksCount) : 0;

                    return Json(new { success = true, percentage = percentage, completedTasksCount = completedTasksCount, totalTasksCount = totalTasksCount });
                }
            }

            // Trả về kết quả lỗi nếu không tìm thấy task
            return Json(new { success = false, error = "Task not found" });
        }
        //Chức năng xóa Task
        public ActionResult DeleteTask(int taskID)
        {
            var deleteTask = iData.Tasks.SingleOrDefault(d => d.TaskID == taskID);
            if (deleteTask != null)
            {
                deleteTask.isDelete = true;
                deleteTask.UpdateDate = DateTime.Now;
                iData.SaveChanges();
                return RedirectToAction("Tasks");
            }
            return View(deleteTask);
        }
        //Chức năng xóa nhiều Task
        public ActionResult DeleteSelectedtask(List<int> taskIDs)
        {
            if (taskIDs != null && taskIDs.Count > 0)
            {
                foreach (var taskID in taskIDs)
                {
                    var existingtask = iData.Tasks.SingleOrDefault(c => c.TaskID == taskID);
                    if (existingtask != null)
                    {
                        existingtask.isDelete = true;
                        existingtask.UpdateDate = DateTime.Now;
                    }
                }
                iData.SaveChanges();
            }
            return RedirectToAction("Companies");
        }
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
        //Chức năng đánh giá Sinh viên
        [HttpGet]
        public ActionResult GetFeedBack(int projectID)
        {
            var existingProject = iData.Projects.SingleOrDefault(e => e.ProjectID == projectID);
            if (existingProject == null)
            {
                return HttpNotFound();
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetFeedBack(Feedback feedBack, int projectID, int instructorID, int studentID)
        {
            if (string.IsNullOrWhiteSpace(feedBack.FeedbackText))
            {
                ModelState.AddModelError("FeedbackText", "Vui lòng nhập đánh giá.");
            }
            if (ModelState.IsValid)
            {
                var checkFeedback = iData.Feedbacks.FirstOrDefault(c => c.ProjectID == projectID);
                if (checkFeedback == null)
                {
                    feedBack.FeedbackDate = DateTime.Now;
                    feedBack.UpdateDate = DateTime.Now;
                    feedBack.ProjectID = projectID;
                    feedBack.StudentID = studentID;
                    feedBack.InstructorID = instructorID;
                    feedBack.isActive = true;
                    feedBack.isDelete = false;
                    iData.Feedbacks.Add(feedBack);
                    iData.SaveChanges();
                }
            }
            return View(feedBack);
        }
        //Chức năng chỉnh sửa Đánh giá 
        [HttpGet]
        public ActionResult EditFeedBack(int feedBackID)
        {
            var existingFeedback = iData.Feedbacks.SingleOrDefault(e => e.FeedbackID == feedBackID);
            if (existingFeedback == null)
            {
                return HttpNotFound();
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditFeedback(int feedbackID, Feedback feedback)
        {
            var updateFeedback = iData.Feedbacks.SingleOrDefault(u => u.FeedbackID == feedbackID);
            if (updateFeedback != null)
            {
                updateFeedback.FeedbackText = feedback.FeedbackText;
                updateFeedback.UpdateDate = DateTime.Now;
                iData.SaveChanges();
            }
            return View(updateFeedback);
        }
        //Chức năng xóa FeedBack 
        public ActionResult DeleteFeedBack(int feedbackID)
        {
            var deleteFeedback = iData.Feedbacks.SingleOrDefault(d => d.FeedbackID == feedbackID);
            if (deleteFeedback != null)
            {
                deleteFeedback.isDelete = true;
                deleteFeedback.isActive = false;
                deleteFeedback.UpdateDate = DateTime.Now;
                iData.SaveChanges();
            }
            return View();
        }
    }
}