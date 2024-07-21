using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace TimerApp
{
    public class TimerContext : DbContext
    {
        public DbSet<TimerApp.Data.Timer> Timer { get; set; }

        public TimerContext(DbContextOptions<TimerContext> options)
           : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
