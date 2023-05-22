using Microsoft.EntityFrameworkCore;
using MovieProfanityDetector.Models.Entities;

namespace MovieProfanityDetector.Data
{
    // Must be expressed in terms of our custom UserRole:


    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public override int SaveChanges() => this.SaveChanges(true);


        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);


            builder.ApplyConfiguration(new BookConfiguration());

            base.OnModelCreating(builder);

        }
    }
}