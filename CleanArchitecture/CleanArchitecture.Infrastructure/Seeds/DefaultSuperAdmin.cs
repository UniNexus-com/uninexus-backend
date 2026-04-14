using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Entities;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Seeds
{
    public static class DefaultSuperAdmin
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Default User
            var defaultUser = new ApplicationUser
            {
                UserName = "sksadmin",
                Email = "admin@akdeniz.edu.tr",
                FullName = "SKS Admin",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };
            if (userManager.Users.All(u => u.UserName != defaultUser.UserName))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Admin123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.SKS_ADMIN.ToString());
                }

            }
        }
    }
}
