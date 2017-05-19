using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenMic.Models
{
    public class Thought
    {
        public int ID { get; set; }
        public int TrackID { get; set; }
        public string ApplicationUserID { get; set; }
        public string Content { get; set; }
        public DateTime date { get; set; }

        virtual public ApplicationUser users { get; set; }
        virtual public Track tracks { get; set; }
    }
}