using System.ComponentModel.DataAnnotations;

namespace icrm.Models
{
    public class SubLocation
    {

        public int Id { get; set; }

        [Required]
        public string name { get; set; }

        public Location location { get; set; }

        [Required]
        public int locationId { get; set; }
    }
}