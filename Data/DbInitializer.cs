using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                // Ensure database is created
                await context.Database.MigrateAsync();

                // Create roles
                await CreateRoles(roleManager);

                // Create admin user
                await CreateAdminUser(userManager);

                // Create sample categories
                await SeedCategories(context);

                // Create sample books
                await SeedBooks(context);

                // Create sample users
                await CreateSampleUsers(userManager);

                logger.LogInformation("Database initialization completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }

        private static async Task CreateRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "Customer" };

            foreach (string roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task CreateAdminUser(UserManager<User> userManager)
        {
            const string adminEmail = "admin@bookstore.com";
            const string adminPassword = "Admin123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Name = "Administrator",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        private static async Task CreateSampleUsers(UserManager<User> userManager)
        {
            var sampleUsers = new[]
            {
                new { Email = "john.doe@example.com", Name = "John Doe", Password = "User123!" },
                new { Email = "jane.smith@example.com", Name = "Jane Smith", Password = "User123!" },
                new { Email = "bob.johnson@example.com", Name = "Bob Johnson", Password = "User123!" }
            };

            foreach (var userData in sampleUsers)
            {
                var existingUser = await userManager.FindByEmailAsync(userData.Email);
                if (existingUser == null)
                {
                    var user = new User
                    {
                        UserName = userData.Email,
                        Email = userData.Email,
                        Name = userData.Name,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(user, userData.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Customer");
                    }
                }
            }
        }

        private static async Task SeedCategories(ApplicationDbContext context)
        {
            if (await context.Categories.AnyAsync())
                return;

            var categories = new[]
            {
                new Category { Name = "Fiction", Description = "Fictional books and novels", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Non-Fiction", Description = "Non-fictional books", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Science", Description = "Science and technology books", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "History", Description = "Historical books", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Biography", Description = "Biographies and memoirs", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Technology", Description = "Technology and programming books", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Children", Description = "Books for children", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Romance", Description = "Romance novels", IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        private static async Task SeedBooks(ApplicationDbContext context)
        {
            if (await context.Books.AnyAsync())
                return;

            var categories = await context.Categories.ToListAsync();
            var random = new Random();

            var books = new[]
            {
                new Book { Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", Price = 12.99m, StockQuantity = 50, CategoryId = categories[0].Id, Publisher = "Scribner", Language = "English", PageCount = 180, Description = "A classic American novel", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Book { Title = "To Kill a Mockingbird", Author = "Harper Lee", Price = 14.99m, DiscountPrice = 11.99m, StockQuantity = 30, CategoryId = categories[0].Id, Publisher = "J.B. Lippincott & Co.", Language = "English", PageCount = 281, Description = "A gripping tale of racial injustice", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Book { Title = "A Brief History of Time", Author = "Stephen Hawking", Price = 18.99m, StockQuantity = 25, CategoryId = categories[2].Id, Publisher = "Bantam", Language = "English", PageCount = 256, Description = "An exploration of cosmology", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Book { Title = "Clean Code", Author = "Robert C. Martin", Price = 45.99m, DiscountPrice = 39.99m, StockQuantity = 40, CategoryId = categories[5].Id, Publisher = "Prentice Hall", Language = "English", PageCount = 464, Description = "A handbook of agile software craftsmanship", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Book { Title = "Steve Jobs", Author = "Walter Isaacson", Price = 16.99m, StockQuantity = 20, CategoryId = categories[4].Id, Publisher = "Simon & Schuster", Language = "English", PageCount = 656, Description = "The exclusive biography", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Book { Title = "Pride and Prejudice", Author = "Jane Austen", Price = 10.99m, StockQuantity = 35, CategoryId = categories[7].Id, Publisher = "Penguin Classics", Language = "English", PageCount = 432, Description = "A romantic novel", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Book { Title = "The Lean Startup", Author = "Eric Ries", Price = 19.99m, StockQuantity = 45, CategoryId = categories[1].Id, Publisher = "Crown Business", Language = "English", PageCount = 336, Description = "How today's entrepreneurs use continuous innovation", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Book { Title = "Harry Potter and the Sorcerer's Stone", Author = "J.K. Rowling", Price = 13.99m, DiscountPrice = 10.99m, StockQuantity = 60, CategoryId = categories[6].Id, Publisher = "Scholastic", Language = "English", PageCount = 309, Description = "The first Harry Potter book", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };

            context.Books.AddRange(books);
            await context.SaveChangesAsync();
        }
    }
}