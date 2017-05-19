using OpenMic.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace OpenMic.DAL
{
    public class OpenMicContext : DbContext
    {
        public OpenMicContext()
            : base("OpenMicContext")
        {
        }
        
        public DbSet<Track> Tracks { get; set; }
        public DbSet<Thought> Thoughts { get; set; }
        

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}