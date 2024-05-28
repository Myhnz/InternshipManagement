using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InternshipManagement.Models
{
    public class ProjectDetailViewModel
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string CompanyName { get; set; }
        public string CompanyLogo { get; set; }
        public string InstructorName { get; set; }
        public List<ProjectMemberViewModel> Members { get; set; }
        public List<TaskViewModel> Tasks { get; set; }
    }

    public class ProjectMemberViewModel
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public string Avatar { get; set; }
    }

    public class TaskViewModel
    {
        public int TaskID { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

}