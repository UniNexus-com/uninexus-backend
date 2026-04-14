using CleanArchitecture.Core.Enums;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Core.Entities;

namespace CleanArchitecture.Infrastructure.Seeds
{
    public static class DefaultBasicUser
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Default User
            var defaultUser = new ApplicationUser
            {
                UserName = "defaultstudent",
                Email = "student@ogr.akdeniz.edu.tr",
                FullName = "Default Student",
                StudentNumber = "20220808099",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };
            if (userManager.Users.All(u => u.UserName != defaultUser.UserName))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Student123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.STUDENT.ToString());
                }

            }
        }
    }
}
