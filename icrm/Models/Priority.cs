using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class Priority
    {
        public int Id { get; set; }

        [Required]
        public int priorityId { get; set; }

        [Required]
        public String name { get; set; }
    }
}