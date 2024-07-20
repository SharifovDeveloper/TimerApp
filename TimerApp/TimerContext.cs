using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace TimerApp
{
    public class TimerContext : DbContext
    {
        public DbSet<Timer> Timer { get; set; }

        public TimerContext(DbContextOptions<TimerContext> options)
           : base(options)
        {
            Database.Migrate();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
