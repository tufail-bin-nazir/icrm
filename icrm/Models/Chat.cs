using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class Chat
    {
        public int Id { get; set; }

        public string UserOneId { get; set; }

        public virtual ApplicationUser UserOne { get; set; }

        public string UserTwoId { get; set; }

        public virtual ApplicationUser UserTwo { get; set; }
    }
}