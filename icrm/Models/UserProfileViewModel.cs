using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class UserProfileViewModel
    {
      
        public string PhoneNumber { get; set; }
        public int LocationId { get; set; }
        public int SubLocationId { get; set; }
        public int PositionId { get; set; }
        public int NationalityId { get; set; }
        public int PayScaleTypeId { get; set; }
        public int ReligionId { get; set; }
        public int JobTitleId { get; set; }
        public int DepartmentId { get; set; }
    }
}