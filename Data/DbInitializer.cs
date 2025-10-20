using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BookStoreMVC.Models.Entities;

namespace BookStoreMVC.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                // Đảm bảo DB được tạo và migrate
                await context.Database.MigrateAsync();

                // Tạo role nếu chưa có
                await CreateRoles(roleManager);

                // Tạo admin duy nhất
                await CreateAdminUser(userManager, configuration, logger);

                logger.LogInformation("✅ Database initialized with admin user only.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error initializing database.");
                throw;
            }
        }

        private static async Task CreateRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin" }; // Chỉ tạo role Admin

            foreach (string roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task CreateAdminUser(UserManager<User> userManager, IConfiguration configuration, ILogger logger)
        {
            // Đọc thông tin admin từ configuration thay vì hardcode
            var email = configuration["AdminAccount:Email"] ?? "admin@bookstore.com";
            var password = configuration["AdminAccount:Password"];
            var name = configuration["AdminAccount:Name"] ?? "Quản trị viên";

            if (string.IsNullOrEmpty(password))
            {
                logger.LogWarning("⚠️ Admin password not configured in appsettings.json. Skipping admin user creation.");
                return;
            }

            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var user = new User
                {
                    UserName = email,
                    Email = email,
                    Name = name,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                    logger.LogInformation("✅ Admin user created successfully with email: {Email}", email);
                }
                else
                {
                    logger.LogError("❌ Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
