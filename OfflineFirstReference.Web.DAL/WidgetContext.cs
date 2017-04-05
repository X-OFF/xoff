using Microsoft.EntityFrameworkCore;
using OfflineFirstReference.Web.DAL.Models;

namespace OfflineFirstReference.Web.DAL
{
    public class WidgetContext : DbContext
    {
        public WidgetContext(DbContextOptions<WidgetContext> options) : base(options)
        {
        }

        public DbSet<Widget> Widgets { get; set; }
          
    }
}