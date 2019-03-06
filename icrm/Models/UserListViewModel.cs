using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class UserListViewModel
    {
        public ApplicationUser user { get; set; }

        public IPagedList<ApplicationUser> users { get; set; }
    }
}