using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class JobTitlesViewModel
    {
        public JobTitle jobTitle { get; set; }
        public IEnumerable<JobTitle> jobTitles { get; set; }
    }
}