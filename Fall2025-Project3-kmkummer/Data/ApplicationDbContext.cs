using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Fall2025_Project3_kmkummer.Models;

namespace Fall2025_Project3_kmkummer.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Fall2025_Project3_kmkummer.Models.Movie> Movie { get; set; } = default!;
        public DbSet<Fall2025_Project3_kmkummer.Models.Actor> Actor { get; set; } = default!;

        public DbSet<Fall2025_Project3_kmkummer.Models.MovieActor> MovieActors { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MovieActor>(entity =>
            {
                entity.HasIndex(ma => new { ma.MovieId, ma.ActorId })
                      .IsUnique(); // prevents duplicate relationships

                entity.HasOne(ma => ma.Movie)
                      .WithMany(m => m.MovieActors)
                      .HasForeignKey(ma => ma.MovieId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ma => ma.Actor)
                      .WithMany(a => a.MovieActors)
                      .HasForeignKey(ma => ma.ActorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }

}
