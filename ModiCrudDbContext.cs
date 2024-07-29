using Microsoft.EntityFrameworkCore;
using ModifiedCrudApp.Models;

namespace ModifiedCrudApp.Data
{
    public class ModiCrudDbContext : DbContext
    {
        public ModiCrudDbContext(DbContextOptions<ModiCrudDbContext> options) : base(options) { }
        public DbSet<Login> Logins { get; set; }
        public DbSet<Register>Registers{ get; set; }
        public DbSet<Employee> Employees { get; set; }
        public override int SaveChanges()
        {
            ConvertEmailsToLower();
            return base.SaveChanges();
        }

        private void ConvertEmailsToLower()
        {
            foreach (var entry in ChangeTracker.Entries<Employee>())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    entry.Entity.Email = entry.Entity.Email?.ToLower();
                }
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure email property to be stored as lowercase
            modelBuilder.Entity<Employee>()
                .Property(e => e.Email)
                .HasConversion(
                    v => v.ToLower(), // Convert to lowercase before saving
                    v => v); // No need to convert on read
        }

    }
}
