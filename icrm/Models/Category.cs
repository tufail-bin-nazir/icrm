using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string name { get; set; }
        [JsonIgnore]
        public IEnumerable<SubCategory> subCategories { get; set; }
        [JsonIgnore]
        public int FeedTypeId { get; set; }
        [JsonIgnore]
        public int DepartmentId { get; set; }

        [JsonIgnore]
        public virtual FeedBackType FeedBackType { get; set; }
    }
}