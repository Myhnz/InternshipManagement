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
    
    public partial class Personalize
    {
        public int PersonalizeID { get; set; }
        public Nullable<int> ProjectID { get; set; }
        public string Color { get; set; }
        public Nullable<bool> Pinned { get; set; }
    
        public virtual Project Project { get; set; }
    }
}
