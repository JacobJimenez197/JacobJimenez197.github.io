using Microsoft.AspNetCore.Identity;
using PlataformaAPI.Models;
using System.Threading.Tasks;

namespace PlataformaAPI
{
    public static class DbInitializer
    {
        public static async Task Initialize(AppDbContext context, UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new Role { Name = "Admin" });
            }

            var adminEmail = "admin@example.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Name = "Administrador",
                    RoleId = (await roleManager.FindByNameAsync("Admin")).Id
                };

                await userManager.CreateAsync(adminUser, "AdminPassword123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}