using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace icrm.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string  LastName { get; set; }

        [Required]
        public int LocationId { get; set; }

        [Required]
        public int SubLocationId { get; set; }

        [Required]
        public int PositionId { get; set; }

        [Required]
        public int NationalityId { get; set; }




        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {


        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            Database.SetInitializer<ApplicationDbContext>(new MyDBInitializer());
        }

        public DbSet<Models.Feedback> Feedbacks { get; set; }

      

        public DbSet<Models.Category> Categories { get; set; }
        public DbSet<Models.Location> Locations { get; set; }
        public DbSet<Models.SubLocation> SubLocations { get; set; }
        public DbSet<Models.Nationality> Nationalities { get; set; }
        public DbSet<Models.Position> Positions { get; set; }
        public DbSet<Models.Priority> Priorities { get; set; }
        public DbSet<Models.JobTitle> JobTitles { get; set; }
        public DbSet<Models.CostCenter> CostCenters { get; set; }
        public DbSet<Models.Religion> Religions { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }

    public class MyDBInitializer : CreateDatabaseIfNotExists<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            // Initialize default identity roles
            var store = new RoleStore<IdentityRole>(context);
            var manager = new RoleManager<IdentityRole>(store);
            // RoleTypes is a class containing constant string values for different roles
            List<IdentityRole> identityRoles = new List<IdentityRole>();
            identityRoles.Add(new IdentityRole() { Name = "Admin" });
            identityRoles.Add(new IdentityRole() { Name = "HR" });
            identityRoles.Add(new IdentityRole() { Name = "User" });
            identityRoles.Add(new IdentityRole() { Name = "Department" });

            foreach (IdentityRole role in identityRoles)
            {
                manager.Create(role);
            }

            // Initialize default user
            var ustore = new UserStore<ApplicationUser>(context);
            var umanager = new UserManager<ApplicationUser>(ustore);
            var admin = new ApplicationUser { UserName = "admin@admin.com", FirstName = "admin", LastName = "admin", Email = "admin@admin.com", PhoneNumber = "1234567890" };


            umanager.Create(admin, "123456");
            umanager.AddToRole(admin.Id, "Admin");

            // Add code to initialize context tables

            base.Seed(context);

        }
    }
}