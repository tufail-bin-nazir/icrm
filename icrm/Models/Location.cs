using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class Location
    {
        public  int Id { get; set; }

        [Required]
        public string name { get; set; }

        public IEnumerable<SubLocation> subLocations { get; set; }
    }
}