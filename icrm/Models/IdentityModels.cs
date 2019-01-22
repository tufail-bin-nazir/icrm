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

        public int EmployeeId { get; set; }

        public int? LocationId { get; set; }

        public virtual Location Location { get; set; }

        public int? SubLocationId { get; set; }

        public virtual SubLocation SubLocation { get; set; }

        public int? PositionId { get; set; }

        public virtual Position Position { get; set; }

        public int? NationalityId { get; set; }

        public virtual Nationality Nationality { get; set; }

        public int? DepartmentId { get; set; }

        public virtual Department Department { get; set; }

        public int? CostCenterId { get; set; }

        public virtual CostCenter CostCenter { get; set; }

        public int? PayScaleTypeId { get; set; }

        public virtual PayScaleType PayScaleType { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public System.DateTime? EmployeeHireDate { get; set; }

        public int? ReligionId { get; set; }

        public virtual Religion Religion { get; set; }

        public int? JobTitleId { get; set; }

        public virtual JobTitle JobTitle { get; set; }

        public bool? status { get; set; }

        public bool? available { get; set; }

        public string DeviceCode { get; set; }


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
        public DbSet<Models.FeedBackType> FeedbackTypes { get; set; }


        public DbSet<Models.Category> Categories { get; set; }
        public DbSet<Models.Location> Locations { get; set; }
        public DbSet<Models.SubLocation> SubLocations { get; set; }
        public DbSet<Models.Nationality> Nationalities { get; set; }
        public DbSet<Models.Position> Positions { get; set; }
        public DbSet<Models.Priority> Priorities { get; set; }
        public DbSet<Models.JobTitle> JobTitles { get; set; }
        public DbSet<Models.CostCenter> CostCenters { get; set; }
        public DbSet<Models.Religion> Religions { get; set; }
        public DbSet<Models.Chat> Chat { get; set; }
        public DbSet<Models.Message> Message { get; set; }
        public DbSet<Models.ChatRequest> ChatRequest { get; set; }
        public DbSet<Models.BroadcastMessage> BroadcastMessage { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<icrm.Models.Department> Departments { get; set; }

        public System.Data.Entity.DbSet<icrm.Models.PayScaleType> PayScaleTypes { get; set; }

        public System.Data.Entity.DbSet<icrm.Models.SubCategory> SubCategories { get; set; }
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
            var admin = new ApplicationUser { EmployeeId=1000, UserName = "admin@admin.com", FirstName = "admin", LastName = "admin", Email = "admin@admin.com", PhoneNumber = "1234567890" };


            umanager.Create(admin, "123456");
            umanager.AddToRole(admin.Id, "Admin");

            // Add code to initialize context tables

            base.Seed(context);

        }
    }
}