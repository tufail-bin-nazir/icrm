using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class PositionsViewModel
    {
        public Position Position { get; set; }
        public IEnumerable<Position> Positions { get; set; }
    }
}