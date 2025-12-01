using Microsoft.EntityFrameworkCore;

namespace BookRenting.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tables
        public DbSet<RegisterUser> RegisterUsers { get; set; }

        public DbSet<Login> Logins { get; set; }

        public DbSet<Admin> Admins { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map Admin entity to existing "Admins" table
            modelBuilder.Entity<Admin>().ToTable("Admins");
        }

         public DbSet<Book> Books { get; set; } = null!;
        public DbSet<RentedBook> RentedBooks { get; set; }

        public DbSet<Report> Reports { get; set; }

public DbSet<ReturnBook> ReturnBooks { get; set; }  // maps to return_books



    }
}
