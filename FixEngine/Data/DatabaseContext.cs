using FixEngine.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FixEngine.Data
{
    public class DatabaseContext : IdentityDbContext<AppUser>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Gateway>().HasIndex(e => e.Name).IsUnique();

            builder.Entity<Group>().HasIndex(e => e.Name).IsUnique();
        }

        public DbSet<Execution> executions { get; set; }
        public DbSet<User> users { get; set; }
        public DbSet<Symbol> symbols { get; set; }
        public DbSet<RiskUser> RiskUsers { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Gateway> Gateways { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Position> Positions { get; set; }
    }
}
