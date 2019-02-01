using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class ChatRequest
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser user { get; set; }
    }
}