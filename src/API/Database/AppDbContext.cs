using API.Database.Models;
using API.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Database
{
    public class AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor _httpContextAccessor) : IdentityDbContext<UserEntity>(options)
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

        // Modificar SaveChangesAsync para capturar automaticamente las props de auditoria (CreatedDate, CreatedBy, UpdatedDate y UpdatedBy)
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Id del usuario
            var userId = _httpContextAccessor.HttpContext?
                .Items[JwtService.HttpContextUserKey] is LogUser auditUser
                ? auditUser.Id
                : null;

            // Fecha actual
            var now = DateTime.UtcNow;

            // Filtrar solo entidades que hereden de BaseEntity
            var entries = ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State is EntityState.Added or EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.CreatedDate = now;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedBy = userId;
                    entry.Entity.UpdatedDate = now;

                    entry.Property(e => e.CreatedBy).IsModified = false;
                    entry.Property(e => e.CreatedDate).IsModified = false;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
