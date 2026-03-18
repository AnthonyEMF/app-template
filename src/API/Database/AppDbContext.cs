using API.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Database
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<UserEntity>(options)
    {
        public DbSet<UserOtpEntity> UsersOtps => Set<UserOtpEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Definir esquema para seguridad
            modelBuilder.HasDefaultSchema("security");

            // Renombrar tablas de IdentityUser
            modelBuilder.Entity<UserEntity>().ToTable("Users");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UsersRoles");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UsersTokens");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UsersClaims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UsersLogins");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RolesClaims");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
