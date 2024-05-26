using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InternshipManagement.Models
{
    public class FormData
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? RoleID { get; set; }
        public string CompanyName { get; set; }
    }
}