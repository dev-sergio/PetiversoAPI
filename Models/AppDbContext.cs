using Microsoft.EntityFrameworkCore;

namespace PetiversoAPI.Models
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Session> Sessions { get; set; } = null!;
        public DbSet<LoginAttempt> LoginAttempts { get; set; } = null!;
        public DbSet<Pet> Pets { get; set; } = null!;
        public DbSet<PetPhoto> PetPhotos { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<Pet>().HasIndex(u => u.PetId).IsUnique();

            modelBuilder.Entity<PetPhoto>().HasKey(pp => pp.PhotoId);

            modelBuilder.Entity<PetPhoto>()
                .HasOne<Pet>(pp => pp.Pet)
                .WithMany(p => p.Photos)
                .HasForeignKey(pp => pp.PetId);
        }
    }
}
