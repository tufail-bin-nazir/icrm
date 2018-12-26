using System.ComponentModel.DataAnnotations;

namespace icrm.Models
{
    public class SubLocation
    {

        public int Id { get; set; }

        [Required]
        public string name { get; set; }
 
        
        public int? LocationId { get; set; }

        public virtual Location  Location{ get; set; }
    }
}