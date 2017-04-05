using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OfflineFirstReference.Web;

namespace OfflineFirstReference.Web.Models
{
    public class OfflineFirstReferenceWebContext : DbContext
    {
        public OfflineFirstReferenceWebContext(DbContextOptions<OfflineFirstReferenceWebContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Widget>().ToTable("Widgets");
        }

        public DbSet<Widget> Widgets { get; set; }
    }

    public static class DbInitializer
    {
        public static void Initialize(OfflineFirstReferenceWebContext context)
        {
            context.Database.EnsureCreated();

        }
    }
}
