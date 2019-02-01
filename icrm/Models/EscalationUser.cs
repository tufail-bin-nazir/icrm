using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class EscalationUser
    {
        public int Id { get; set; }
        public ApplicationUser firstEscalationUser { get; set; }
        public ApplicationUser secondEscalationUser { get; set; }
        public ApplicationUser thirdEscalationUser { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public int DepartmentId  { get; set; }
    }
}