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

                // Create sample categories with hierarchy
                await SeedCategories(context);

                // Create comprehensive book collection
                await SeedBooks(context);

                // Create diverse sample users
                await CreateSampleUsers(userManager);

                // Create sample reviews
                await SeedReviews(context);

                // Create sample orders (optional)
                await SeedSampleOrders(context);

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
            string[] roleNames = { "Admin", "Customer", "Manager", "Editor" };

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
            var adminUsers = new[]
            {
                new { Email = "admin@bookstore.com", Name = "Quản trị viên", Role = "Admin", Password = "Admin123!" },
                new { Email = "manager@bookstore.com", Name = "Nguyễn Văn Quản", Role = "Manager", Password = "Manager123!" },
                new { Email = "editor@bookstore.com", Name = "Trần Thị Biên tập", Role = "Editor", Password = "Editor123!" }
            };

            foreach (var adminData in adminUsers)
            {
                var existingUser = await userManager.FindByEmailAsync(adminData.Email);
                if (existingUser == null)
                {
                    var user = new User
                    {
                        UserName = adminData.Email,
                        Email = adminData.Email,
                        Name = adminData.Name,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(user, adminData.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, adminData.Role);
                    }
                }
            }
        }

        private static async Task CreateSampleUsers(UserManager<User> userManager)
        {
            var sampleUsers = new[]
            {
                new { Email = "nguyenvana@gmail.com", Name = "Nguyễn Văn A", Password = "User123!" },
                new { Email = "tranthib@gmail.com", Name = "Trần Thị B", Password = "User123!" },
                new { Email = "lequangc@gmail.com", Name = "Lê Quang C", Password = "User123!" },
                new { Email = "phamthid@gmail.com", Name = "Phạm Thị D", Password = "User123!" },
                new { Email = "hoangvane@gmail.com", Name = "Hoàng Văn E", Password = "User123!" },
                new { Email = "vuthif@gmail.com", Name = "Vũ Thị F", Password = "User123!" },
                new { Email = "dangvanG@gmail.com", Name = "Đặng Văn G", Password = "User123!" },
                new { Email = "buithi@gmail.com", Name = "Bùi Thị H", Password = "User123!" }
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
                        CreatedAt = DateTime.UtcNow.AddDays(-new Random().Next(1, 365))
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
                // Main categories
                new Category { Name = "Văn học trong nước", Description = "Tác phẩm văn học của tác giả Việt Nam", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Văn học nước ngoài", Description = "Tác phẩm văn học dịch từ các nước", IsActive = true,  CreatedAt = DateTime.UtcNow },
                new Category { Name = "Kinh tế - Quản lý", Description = "Sách về kinh doanh, quản lý, đầu tư", IsActive = true,  CreatedAt = DateTime.UtcNow },
                new Category { Name = "Kỹ năng sống", Description = "Phát triển bản thân, kỹ năng mềm", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Khoa học - Công nghệ", Description = "Sách khoa học, công nghệ, lập trình", IsActive = true,  CreatedAt = DateTime.UtcNow },
                new Category { Name = "Lịch sử - Chính trị", Description = "Sách lịch sử, chính trị, xã hội", IsActive = true,  CreatedAt = DateTime.UtcNow },
                new Category { Name = "Tâm lý học", Description = "Sách về tâm lý, hành vi, con người", IsActive = true,  CreatedAt = DateTime.UtcNow },
                new Category { Name = "Thiếu nhi", Description = "Sách dành cho trẻ em và thiếu niên", IsActive = true,  CreatedAt = DateTime.UtcNow },
                new Category { Name = "Giáo trình", Description = "Sách giáo khoa, tham khảo học tập", IsActive = true,  CreatedAt = DateTime.UtcNow },
                new Category { Name = "Y học - Sức khỏe", Description = "Sách về y học, chăm sóc sức khỏe", IsActive = true,  CreatedAt = DateTime.UtcNow },
                new Category { Name = "Nghệ thuật - Giải trí", Description = "Sách về nghệ thuật, âm nhạc, điện ảnh", IsActive = true,  CreatedAt = DateTime.UtcNow },
                new Category { Name = "Nấu ăn - Ẩm thực", Description = "Sách dạy nấu ăn, văn hóa ẩm thực", IsActive = true,  CreatedAt = DateTime.UtcNow }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        private static async Task SeedBooks(ApplicationDbContext context)
        {
            if (await context.Books.AnyAsync())
                return;

            var categories = await context.Categories.ToDictionaryAsync(c => c.Name, c => c.Id);
            var random = new Random();

            var books = new[]
            {
                // Văn học trong nước
                new Book
                {
                    Title = "Số đỏ",
                    Author = "Vũ Trọng Phụng",
                    Price = 89000m,
                    StockQuantity = 45,
                    CategoryId = categories["Văn học trong nước"],
                    Publisher = "NXB Văn học",
                    Language = "Tiếng Việt",
                    PageCount = 320,
                    Description = "Tác phẩm kinh điển của văn học Việt Nam, miêu tả xã hội Hà Nội thập niên 30 của thế kỷ XX",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                },
                new Book
                {
                    Title = "Dế Mèn phiêu lưu ký",
                    Author = "Tô Hoài",
                    Price = 55000m,
                    DiscountPrice = 45000m,
                    StockQuantity = 80,
                    CategoryId = categories["Văn học trong nước"],
                    Publisher = "NXB Kim Đồng",
                    Language = "Tiếng Việt",
                    PageCount = 180,
                    Description = "Tác phẩm nổi tiếng dành cho thiếu nhi của nhà văn Tô Hoài",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                },
                new Book
                {
                    Title = "Tắt đèn",
                    Author = "Ngô Tất Tố",
                    Price = 75000m,
                    StockQuantity = 25,
                    CategoryId = categories["Văn học trong nước"],
                    Publisher = "NXB Văn học",
                    Language = "Tiếng Việt",
                    PageCount = 280,
                    Description = "Tiểu thuyết hiện thực phê phán nổi tiếng về cuộc sống nông thôn Việt Nam",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                },

                // Văn học nước ngoài  
                new Book
                {
                    Title = "Nhà giả kim",
                    Author = "Paulo Coelho",
                    Price = 98000m,
                    DiscountPrice = 78000m,
                    StockQuantity = 120,
                    CategoryId = categories["Văn học nước ngoài"],
                    Publisher = "NXB Hội Nhà văn",
                    Language = "Tiếng Việt",
                    PageCount = 220,
                    Description = "Câu chuyện về hành trình tìm kiếm ước mơ của cậu bé chăn cừu Santiago",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                },
                new Book
                {
                    Title = "Tiểu thuyết Mùa hè không tên",
                    Author = "Nguyễn Nhật Ánh",
                    Price = 85000m,
                    StockQuantity = 95,
                    CategoryId = categories["Văn học nước ngoài"],
                    Publisher = "NXB Trẻ",
                    Language = "Tiếng Việt",
                    PageCount = 245,
                    Description = "Tác phẩm mới nhất của nhà văn được yêu thích Nguyễn Nhật Ánh",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                },
                new Book
                {
                    Title = "1984",
                    Author = "George Orwell",
                    Price = 125000m,
                    DiscountPrice = 99000m,
                    StockQuantity = 60,
                    CategoryId = categories["Văn học nước ngoài"],
                    Publisher = "NXB Văn học",
                    Language = "Tiếng Việt",
                    PageCount = 380,
                    Description = "Tiểu thuyết dystopia kinh điển về xã hội toàn trị",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                },

                // Kinh tế - Quản lý
                new Book
                {
                    Title = "Dạy con làm giàu tập 1",
                    Author = "Robert Kiyosaki",
                    Price = 189000m,
                    DiscountPrice = 149000m,
                    StockQuantity = 40,
                    CategoryId = categories["Kinh tế - Quản lý"],
                    Publisher = "NXB Thế giới",
                    Language = "Tiếng Việt",
                    PageCount = 280,
                    Description = "Cuốn sách kinh điển về tư duy tài chính và đầu tư",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                },
                new Book
                {
                    Title = "Startup vừa đủ",
                    Author = "Dustin Moskovitz",
                    Price = 245000m,
                    StockQuantity = 30,
                    CategoryId = categories["Kinh tế - Quản lý"],
                    Publisher = "NXB Trẻ",
                    Language = "Tiếng Việt",
                    PageCount = 320,
                    Description = "Hướng dẫn xây dựng startup hiệu quả mà không cần quá nhiều vốn",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                },

                // Kỹ năng sống
                new Book
                {
                    Title = "Đắc nhân tâm",
                    Author = "Dale Carnegie",
                    Price = 156000m,
                    DiscountPrice = 125000m,
                    StockQuantity = 150,
                    CategoryId = categories["Kỹ năng sống"],
                    Publisher = "NXB Tổng hợp TP.HCM",
                    Language = "Tiếng Việt",
                    PageCount = 320,
                    Description = "Cuốn sách kinh điển về nghệ thuật giao tiếp và ứng xử",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                },
                new Book
                {
                    Title = "Atomic Habits",
                    Author = "James Clear",
                    Price = 199000m,
                    DiscountPrice = 159000m,
                    StockQuantity = 75,
                    CategoryId = categories["Kỹ năng sống"],
                    Publisher = "NXB Thế giới",
                    Language = "Tiếng Việt",
                    PageCount = 380,
                    Description = "Cách thức xây dựng thói quen tích cực và phá bỏ thói quen xấu",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                },

                // Khoa học - Công nghệ
                new Book
                {
                    Title = "Clean Code - Mã nguồn sạch",
                    Author = "Robert C. Martin",
                    Price = 450000m,
                    DiscountPrice = 380000m,
                    StockQuantity = 25,
                    CategoryId = categories["Khoa học - Công nghệ"],
                    Publisher = "NXB Thanh niên",
                    Language = "Tiếng Việt",
                    PageCount = 480,
                    Description = "Hướng dẫn viết code chất lượng cho lập trình viên",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                },
                new Book
                {
                    Title = "Lập trình Python từ cơ bản đến nâng cao",
                    Author = "Nguyễn Việt Hùng",
                    Price = 320000m,
                    StockQuantity = 35,
                    CategoryId = categories["Khoa học - Công nghệ"],
                    Publisher = "NXB Bách khoa Hà Nội",
                    Language = "Tiếng Việt",
                    PageCount = 520,
                    Description = "Giáo trình toàn diện về ngôn ngữ lập trình Python",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                },

                // Tâm lý học
                new Book
                {
                    Title = "Tâm lý học tích cực",
                    Author = "Martin Seligman",
                    Price = 185000m,
                    DiscountPrice = 145000m,
                    StockQuantity = 50,
                    CategoryId = categories["Tâm lý học"],
                    Publisher = "NXB Thế giới",
                    Language = "Tiếng Việt",
                    PageCount = 350,
                    Description = "Khám phá khoa học về hạnh phúc và sự thịnh vượng",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                },
                
                // Thiếu nhi
                new Book
                {
                    Title = "Doraemon Nobita và vương quốc robot",
                    Author = "Fujiko F. Fujio",
                    Price = 45000m,
                    StockQuantity = 200,
                    CategoryId = categories["Thiếu nhi"],
                    Publisher = "NXB Kim Đồng",
                    Language = "Tiếng Việt",
                    PageCount = 180,
                    Description = "Truyện tranh nổi tiếng dành cho thiếu nhi",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                },

                // Y học - Sức khỏe
                new Book
                {
                    Title = "Cẩm nang chăm sóc sức khỏe gia đình",
                    Author = "BS. Nguyễn Văn Dũng",
                    Price = 275000m,
                    StockQuantity = 40,
                    CategoryId = categories["Y học - Sức khỏe"],
                    Publisher = "NXB Y học",
                    Language = "Tiếng Việt",
                    PageCount = 420,
                    Description = "Hướng dẫn chăm sóc sức khỏe cho mọi lứa tuổi",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                },

                // Nấu ăn - Ẩm thực
                new Book
                {
                    Title = "500 món ăn ngon mỗi ngày",
                    Author = "Christine Vịt Đức",
                    Price = 165000m,
                    DiscountPrice = 135000m,
                    StockQuantity = 60,
                    CategoryId = categories["Nấu ăn - Ẩm thực"],
                    Publisher = "NXB Phụ nữ",
                    Language = "Tiếng Việt",
                    PageCount = 300,
                    Description = "Tuyển tập các món ăn đa dạng, dễ làm cho gia đình",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                }
            };

            context.Books.AddRange(books);
            await context.SaveChangesAsync();
        }

        private static async Task SeedReviews(ApplicationDbContext context)
        {
            if (await context.Reviews.AnyAsync())
                return;

            var users = await context.Users.Where(u => !u.Email.Contains("admin") && !u.Email.Contains("manager")).Take(5).ToListAsync();
            var books = await context.Books.Take(10).ToListAsync();
            var random = new Random();

            if (!users.Any() || !books.Any()) return;

            var reviews = new List<Review>();

            foreach (var book in books.Take(6)) // Add reviews for first 6 books
            {
                var numReviews = random.Next(1, 4); // 1-3 reviews per book
                var selectedUsers = users.OrderBy(x => random.Next()).Take(numReviews);

                foreach (var user in selectedUsers)
                {
                    var rating = random.Next(3, 6); // Rating from 3-5
                    var comments = new[]
                    {
                        "Cuốn sách rất hay và bổ ích!",
                        "Nội dung thú vị, dễ hiểu.",
                        "Chất lượng sách tốt, giao hàng nhanh.",
                        "Đọc rất cuốn hút, không thể rời mắt.",
                        "Kiến thức hữu ích cho công việc.",
                        "Phù hợp để đọc trong thời gian rảnh.",
                        "Sách hay, đóng gói cẩn thận.",
                        "Tác giả viết rất sinh động và hấp dẫn."
                    };

                    reviews.Add(new Review
                    {
                        BookId = book.Id,
                        UserId = user.Id,
                        Rating = rating,
                        Comment = comments[random.Next(comments.Length)],
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 90))
                    });
                }
            }

            context.Reviews.AddRange(reviews);
            await context.SaveChangesAsync();
        }

        private static async Task SeedSampleOrders(ApplicationDbContext context)
        {
            if (await context.Orders.AnyAsync())
                return;

            var users = await context.Users.Where(u => !u.Email.Contains("admin") && !u.Email.Contains("manager")).Take(5).ToListAsync();
            var books = await context.Books.ToListAsync();
            var random = new Random();

            if (!users.Any() || !books.Any()) return;

            var orders = new List<Order>();

            foreach (var user in users.Take(3)) // Create orders for first 3 customers
            {
                var numOrders = random.Next(1, 3); // 1-2 orders per user

                for (int i = 0; i < numOrders; i++)
                {
                    var orderDate = DateTime.UtcNow.AddDays(-random.Next(1, 60));
                    var status = (OrderStatus)random.Next(1, 4); // Pending, Processing, Completed

                    var order = new Order
                    {
                        UserId = user.Id,
                        Id = (int)(DateTime.Now.Ticks % int.MaxValue),
                        Status = status,
                        CreatedAt = orderDate,
                    };

                    // Add 1-3 items to each order
                    var selectedBooks = books.OrderBy(x => random.Next()).Take(random.Next(1, 4));
                    decimal subTotal = 0;

                    var orderItems = new List<OrderItem>();
                    foreach (var book in selectedBooks)
                    {
                        var quantity = random.Next(1, 3);
                        var unitPrice = book.DiscountPrice ?? book.Price;
                        var total = quantity * unitPrice;
                        subTotal += total;

                        orderItems.Add(new OrderItem
                        {
                            BookId = book.Id,
                            Quantity = quantity,
                            UnitPrice = unitPrice,
                            Total = total
                        });
                    }

                    order.OrderItems = orderItems;
                    order.SubTotal = subTotal;
                    order.Tax = subTotal * 0.1m; // 10% tax
                    order.Total = order.SubTotal + order.Tax;

                    orders.Add(order);
                }
            }

            context.Orders.AddRange(orders);
            await context.SaveChangesAsync();
        }
    }
}