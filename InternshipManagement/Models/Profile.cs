//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace InternshipManagement.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Profile
    {
        public int ProfileID { get; set; }
        public Nullable<int> UserID { get; set; }
        public string Bio { get; set; }
        public string Certifications { get; set; }
        public string Website { get; set; }
        public string Experience { get; set; }
        public string Skills { get; set; }
        public Nullable<System.DateTime> CreatedAt { get; set; }
        public Nullable<System.DateTime> UpdatedAt { get; set; }
    
        public virtual User User { get; set; }
    }
}
