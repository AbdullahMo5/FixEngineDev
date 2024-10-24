using FixEngine.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FixEngine.Data
{
    public class DatabaseContext: IdentityDbContext<AppUser>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
                
        }
        public DbSet<Execution> executions { get; set; }
        public DbSet<User> users { get; set; }
        public DbSet<Symbol> symbols { get; set; }
    
    }
}
