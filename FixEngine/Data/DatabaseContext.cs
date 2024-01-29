using FixEngine.Entity;
using Microsoft.EntityFrameworkCore;

namespace FixEngine.Data
{
    public class DatabaseContext: DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
                
        }
        public DbSet<Execution> executions { get; set; }
        public DbSet<User> users { get; set; }
    
    }
}
