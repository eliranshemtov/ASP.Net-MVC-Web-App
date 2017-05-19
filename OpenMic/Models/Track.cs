using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OpenMic.Models
{
    public class Track
    {
        public int ID { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Video { get; set; }
        public string ArtistsID { get; set; }
        public DateTime Date { get; set; }
        [Required]
        public string Genre { get; set; }
        public int? Trash { get; set; }
        public int? Gold { get; set; }
        public int? TotalRankers { get; set; }


        virtual public ApplicationUser Artists { get; set; }
        virtual public ICollection<Thought> Thoughts { get; set; }
    }
}