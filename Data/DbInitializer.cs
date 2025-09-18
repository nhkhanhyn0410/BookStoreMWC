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

                // Create sample users
                await CreateSampleUsers(userManager);

                // Seed additional data if needed
                await SeedAdditionalData(context);

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
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true,
                    IsActive = true,
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
                new { Email = "john.doe@example.com", FirstName = "John", LastName = "Doe" },
                new { Email = "jane.smith@example.com", FirstName = "Jane", LastName = "Smith" },
                new { Email = "mike.johnson@example.com", FirstName = "Mike", LastName = "Johnson" }
            };

            foreach (var userData in sampleUsers)
            {
                var user = await userManager.FindByEmailAsync(userData.Email);

                if (user == null)
                {
                    user = new User
                    {
                        UserName = userData.Email,
                        Email = userData.Email,
                        FirstName = userData.FirstName,
                        LastName = userData.LastName,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(user, "Customer123!");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Customer");
                    }
                }
            }
        }

        private static async Task SeedAdditionalData(ApplicationDbContext context)
        {
            // Seed categories trước
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
        {
            new Category { NameCategory = "Programming" },
            new Category { NameCategory = "Fiction" },
            new Category { NameCategory = "Self-help" },
            new Category { NameCategory = "Fantasy" },
            new Category { NameCategory = "Mystery" }
        };

                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // Lấy categories ra để gán cho Book
            var categoriesDict = context.Categories.ToDictionary(c => c.NameCategory, c => c.Id);

            if (!context.Books.Any())
            {
                var additionalBooks = new List<Book>
        {
            new Book
            {
                Title = "Clean Code",
                Author = "Robert C. Martin",
                Size = "9780132350884",
                Description = "A Handbook of Agile Software Craftsmanship",
                Price = 29.99m,
                DiscountPrice = 24.99m,
                CategoryId = categoriesDict["Programming"],
                Publisher = "Prentice Hall",
                PageCount = 464,
                Language = "English",
                StockQuantity = 20,
                IsActive = true,
                ImageUrl = "/images/books/clean-code.jpg"
            },
            new Book
            {
                Title = "The Alchemist",
                Author = "Paulo Coelho",
                Size = "9780062315007",
                Description = "A magical story about Santiago, an Andalusian shepherd boy",
                Price = 14.99m,
                CategoryId = categoriesDict["Fiction"],
                Publisher = "HarperOne",
                PageCount = 163,
                Language = "English",
                StockQuantity = 60,
                IsActive = true,
                ImageUrl = "/images/books/alchemist.jpg"
            },
            new Book
            {
                Title = "Think and Grow Rich",
                Author = "Napoleon Hill",
                Size = "9781585424331",
                Description = "The classic guide to wealth and success",
                Price = 12.99m,
                DiscountPrice = 9.99m,
                CategoryId = categoriesDict["Self-help"],
                Publisher = "TarcherPerigee",
                PageCount = 320,
                Language = "English",
                StockQuantity = 35,
                IsActive = true,
                ImageUrl = "/images/books/think-grow-rich.jpg"
            },
            new Book
            {
                Title = "Harry Potter and the Philosopher's Stone",
                Author = "J.K. Rowling",
                Size = "9780439708180",
                Description = "The first book in the Harry Potter series",
                Price = 15.99m,
                CategoryId = categoriesDict["Fantasy"],
                Publisher = "Scholastic",
                PageCount = 309,
                Language = "English",
                StockQuantity = 100,
                IsActive = true,
                ImageUrl = "/images/books/harry-potter-1.jpg"
            },
            new Book
            {
                Title = "The Da Vinci Code",
                Author = "Dan Brown",
                Size = "9780307474278",
                Description = "A mystery thriller novel",
                Price = 16.99m,
                DiscountPrice = 13.99m,
                CategoryId = categoriesDict["Mystery"],
                Publisher = "Anchor",
                PageCount = 689,
                Language = "English",
                StockQuantity = 45,
                IsActive = true,
                ImageUrl = "/images/books/da-vinci-code.jpg"
            }
        };

                context.Books.AddRange(additionalBooks);
                await context.SaveChangesAsync();
            }

            // Add sample reviews
            if (!context.Reviews.Any())
            {
                var users = await context.Users.Where(u => u.Email != "admin@bookstore.com").ToListAsync();
                var books = await context.Books.Take(5).ToListAsync();

                if (users.Any() && books.Any())
                {
                    var reviews = new List<Review>();
                    var random = new Random();

                    foreach (var book in books)
                    {
                        for (int i = 0; i < random.Next(1, 4); i++)
                        {
                            var user = users[random.Next(users.Count)];

                            if (!reviews.Any(r => r.BookId == book.Id && r.UserId == user.Id))
                            {
                                reviews.Add(new Review
                                {
                                    BookId = book.Id,
                                    UserId = user.Id,
                                    Rating = random.Next(3, 6),
                                    Title = $"Great book review for {book.Title}",
                                    Comment = "This is a sample review comment. The book was really interesting and well-written.",
                                    IsVerifiedPurchase = true,
                                    IsApproved = true,
                                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                                });
                            }
                        }
                    }

                    context.Reviews.AddRange(reviews);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}