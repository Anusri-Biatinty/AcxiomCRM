using AcxiomCRM.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace AcxiomCRM.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Create roles
            string[] roles = { "Admin", "Manager", "SalesExecutive" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed default admin
            const string adminEmail    = "admin@acxiomcrm.com";
            const string adminPassword = "Admin@123456";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName       = adminEmail,
                    Email          = adminEmail,
                    FullName       = "System Admin",
                    IsActive       = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Seed demo Manager
            const string managerEmail    = "manager@acxiomcrm.com";
            const string managerPassword = "Manager@123456";

            if (await userManager.FindByEmailAsync(managerEmail) == null)
            {
                var manager = new ApplicationUser
                {
                    UserName       = managerEmail,
                    Email          = managerEmail,
                    FullName       = "Sales Manager",
                    IsActive       = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(manager, managerPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(manager, "Manager");
            }

            // Seed demo SalesExecutive
            const string salesEmail    = "sales@acxiomcrm.com";
            const string salesPassword = "Sales@123456";

            if (await userManager.FindByEmailAsync(salesEmail) == null)
            {
                var sales = new ApplicationUser
                {
                    UserName       = salesEmail,
                    Email          = salesEmail,
                    FullName       = "Sales Executive",
                    IsActive       = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(sales, salesPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(sales, "SalesExecutive");
            }
        }
    }
}
