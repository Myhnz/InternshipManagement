using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace InternshipManagement.Models
{
    public class EditCVViewModel
    {
        public User User { get; set; }
        public Profile Profile { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public HttpPostedFileBase AvatarImage { get; set; }
    }
}