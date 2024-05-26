using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InternshipManagement.Models
{
    public class ProjectViewModel
    {
        public int ProjectID { get; set; }
        public string CompanyName { get; set; }
        public string Logo { get; set; } 
        public string InstructorName { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public string LikesCount { get; set; }
        public DateTime CreateDate { get; set; }
    }
}