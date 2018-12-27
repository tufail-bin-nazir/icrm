using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class PayScaleTypeViewModel
    {
        public PayScaleType PayScaleType { get; set; }

       
        public IEnumerable<PayScaleType> PayScaleTypes { get; set; }
    }
}