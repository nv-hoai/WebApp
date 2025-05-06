using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace FastFood.MVC.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser() : base()
        {
        }

        public ApplicationUser(string userName) : base(userName)
        {
        }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ApplicationUserClaim> Claims { get; set; } = new HashSet<ApplicationUserClaim>();
        public virtual ICollection<ApplicationUserLogin> Logins { get; set; } = new HashSet<ApplicationUserLogin>();
        public virtual ICollection<ApplicationUserToken> Tokens { get; set; } = new HashSet<ApplicationUserToken>();
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = new HashSet<ApplicationUserRole>();

        [JsonIgnore]
        public virtual Admin? Admin { get; set; }
        [JsonIgnore]
        public virtual Customer? Customer { get; set; }
        [JsonIgnore]
        public virtual Employee? Employee { get; set; }
        [JsonIgnore]
        public virtual Shipper? Shipper { get; set; }
    }

    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base()
        {
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
        }

        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = new HashSet<ApplicationUserRole>();
        public virtual ICollection<ApplicationRoleClaim> RoleClaims { get; set; } = new HashSet<ApplicationRoleClaim>();
    }

    public class ApplicationUserRole : IdentityUserRole<string>
    {
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ApplicationRole Role { get; set; } = null!;
    }

    public class ApplicationUserClaim : IdentityUserClaim<string>
    {
        public virtual ApplicationUser User { get; set; } = null!;
    }

    public class ApplicationUserLogin : IdentityUserLogin<string>
    {
        public virtual ApplicationUser User { get; set; } = null!;
    }

    public class ApplicationRoleClaim : IdentityRoleClaim<string>
    {
        public virtual ApplicationRole Role { get; set; } = null!;
    }

    public class ApplicationUserToken : IdentityUserToken<string>
    {
        public virtual ApplicationUser User { get; set; } = null!;
    }
}