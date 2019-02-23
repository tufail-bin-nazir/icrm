using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
	public class CostCenter
	{
        public int Id { get; set; }
        public string CostCenterCode { get; set; }
        public String name { get; set; }
        public String costCenterDisplay { get { return string.Format("{0} {1}", name, CostCenterCode); } }
    }
}