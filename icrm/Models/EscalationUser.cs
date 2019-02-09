using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class EscalationUser
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public int Id { get; set; }
        public string firstEscalationUserId { get; set; }
        public ApplicationUser firstEscalationUser { get; set; }
        public string secondEscalationUserId { get; set; }
        public ApplicationUser secondEscalationUser { get; set; }
        public string thirdEscalationUserId { get; set; }
        public ApplicationUser thirdEscalationUser { get; set; }
        public IEnumerable<int> CategoriesIds { get; set; }
        public int DepartmentId  { get; set; }
        public virtual Department Department { get; set; }
    }
}