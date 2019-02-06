using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace icrm.Models
{
    public class Chat
    {
        public int Id { get; set; }

        public string UserOneId { get; set; }
        [JsonIgnore]
        public virtual ApplicationUser UserOne { get; set; }

        public string UserTwoId { get; set; }
        [JsonIgnore]
        public virtual ApplicationUser UserTwo { get; set; }

        public bool active { get; set; }

        public override string ToString()
        {
            return $"{nameof(this.Id)}: {this.Id}, {nameof(this.UserOneId)}: {this.UserOneId}, {nameof(this.UserOne)}: {this.UserOne}, {nameof(this.UserTwoId)}: {this.UserTwoId}, {nameof(this.UserTwo)}: {this.UserTwo}, {nameof(this.active)}: {this.active}";
        }
    }
}