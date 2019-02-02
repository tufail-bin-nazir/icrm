using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class SubCategoryViewModel
    {
        public SubCategory subCategory { get; set; }
        public IEnumerable<SubCategory> subCategories { get; set; }
    }
}