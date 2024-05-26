using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InternshipManagement.Models
{
    public class ProjectDetails
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public int StudentsCount { get; set; }
        public string BackgroundPicture { get; set; }
        public string Description { get; set; }
        public int MaxStudents { get; set; }
        public int LikesCount { get; set; }
        public int CompanyID { get; set; }
        public string CompanyName { get; set; }
        public int InstructorID { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public ICollection<Student> Members { get; set; }
        public ICollection<Task> Tasks { get; set; }
        public ICollection<Student> QueueStudents { get; set; }
        public Personalize Personalize { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string InstructorFirstName { get; set; }
        public string InstructorLastName { get; set; }
    }
}