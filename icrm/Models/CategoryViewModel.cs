using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class CategoryViewModel
    {
        public Category Category { get; set; }
        public IPagedList<Category> Categories { get; set; }
    }
}