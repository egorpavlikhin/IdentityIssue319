using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.OptionsModel;

namespace IdentityIssue319.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {

    }

    public class IdentityDbContextOptions : DbContextOptions
    {
        public string DefaultAdminUserName { get; set; }

        public string DefaultAdminPassword { get; set; }
    }
}