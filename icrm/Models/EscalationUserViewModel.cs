using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class EscalationUserViewModel
    {
        public EscalationUser EscalationUser { get; set; }
        public IEnumerable<EscalationUser> EscalationUsers { get; set; }
    }
}