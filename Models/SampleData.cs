using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Data.Entity.SqlServer;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;

namespace IdentityIssue319.Models
{
    public static class SampleData
    {
        public static async Task InitializeIdentityDatabaseAsync(IServiceProvider serviceProvider)
        {
            using (var db = serviceProvider.GetRequiredService<ApplicationDbContext>())
            {
                var sqlServerDatabase = db.Database as SqlServerDatabase;
                if (sqlServerDatabase != null)
                {
                    if (await sqlServerDatabase.ExistsAsync() && await sqlServerDatabase.HasTablesAsync())
                    {
                        await CreateAdminUser(serviceProvider);
                    }
                }
                else
                {
                    await CreateAdminUser(serviceProvider);
                }
            }
        }

        /// <summary>
        /// Creates a store manager user who can manage the inventory.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        private static async Task CreateAdminUser(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetRequiredService<IOptions<IdentityDbContextOptions>>().Options;
            const string adminRole = "Administrator";

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            var user = await userManager.FindByNameAsync(options.DefaultAdminUserName);
	        if (user != null)
	        {
		        var claims = await userManager.GetClaimsAsync(user);
		        await userManager.RemoveClaimsAsync(user, claims);
		        await userManager.RemoveLoginAsync(user, "test", "test");
		        await userManager.DeleteAsync(user);
	        }

			user = await userManager.FindByNameAsync(options.DefaultAdminUserName);
			if (user == null)
            {
                user = new ApplicationUser { UserName = options.DefaultAdminUserName };
                await userManager.CreateAsync(user, options.DefaultAdminPassword);
                await userManager.AddToRoleAsync(user, adminRole);
                await userManager.AddClaimAsync(user, new Claim("ManageStore", "Allowed"));
	            await userManager.AddLoginAsync(user, new UserLoginInfo("test", "test", "Administrator"));
            }
        }
    }
}