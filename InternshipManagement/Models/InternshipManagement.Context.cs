﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class InternshipManagementEntities : DbContext
    {
        public InternshipManagementEntities()
            : base("name=InternshipManagementEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Administrator> Administrators { get; set; }
        public virtual DbSet<Company> Companies { get; set; }
        public virtual DbSet<DeletedUser> DeletedUsers { get; set; }
        public virtual DbSet<FavoriteProject> FavoriteProjects { get; set; }
        public virtual DbSet<Feedback> Feedbacks { get; set; }
        public virtual DbSet<Instructor> Instructors { get; set; }
        public virtual DbSet<InternshipInformation> InternshipInformations { get; set; }
        public virtual DbSet<InternshipQueue> InternshipQueues { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<ProjectSpecialization> ProjectSpecializations { get; set; }
        public virtual DbSet<ProjectTag> ProjectTags { get; set; }
        public virtual DbSet<Specialization> Specializations { get; set; }
        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<Task> Tasks { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Personalize> Personalizes { get; set; }
        public virtual DbSet<Profile> Profiles { get; set; }
        public virtual DbSet<LoginHistory> LoginHistories { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
    }
}
