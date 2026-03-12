using API.Constants;
using API.Database.Entities;
using API.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace API.Services.Seed
{
    public class SeedService(RoleManager<IdentityRole> _roleManager, UserManager<UserEntity> _userManager) : ISeedService
    {
        // Cargar roles (RolesConstant.cs)
        public async Task LoadRolesAsync()
        {
            var roles = new[] { RolesConstant.ADMIN, RolesConstant.USER };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Cargar usuarios (users.json)
        public async Task LoadUsersAsync()
        {
            var jsonFilePath = "Database/SeedData/users.json";
            var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
            var users = JsonConvert.DeserializeObject<List<RegisterReqDto>>(jsonContent);

            foreach (var seedUser in users)
            {
                // Saltar si el usuario ya existe
                if (await _userManager.FindByNameAsync(seedUser.UserName) is not null)
                    continue;

                var user = new UserEntity
                {
                    FirstName = seedUser.FirstName,
                    LastName = seedUser.LastName,
                    UserName = seedUser.UserName,
                    Email = seedUser.Email,
                    CreatedDate = DateTime.UtcNow,
                };

                // Crear los usuarios y definir la misma contraseña para todos
                var result = await _userManager.CreateAsync(user, "Temporal01!");

                // Asignar los roles
                if (result.Succeeded)
                    await _userManager.AddToRoleAsync(user, seedUser.Role);
            }
        }
    }
}
