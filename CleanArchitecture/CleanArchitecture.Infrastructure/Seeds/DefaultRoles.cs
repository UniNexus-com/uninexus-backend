using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Entities;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Seeds
{
    public static class DefaultRoles
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Roles
            await roleManager.CreateAsync(new IdentityRole(Roles.SKS_ADMIN.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.CLUB_LEADER.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.STUDENT.ToString()));
        }
    }
}
