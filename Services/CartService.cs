// Services/ICartService.cs & CartService.cs
using Microsoft.EntityFrameworkCore;
using BookStoreMVC.Data;
using BookStoreMVC.Models.Entities;
using BookStoreMVC.Models.ViewModels;

namespace BookStoreMVC.Services
{
    public interface ICartService
    {
        Task<CartViewModel> GetCartAsync(string userId);
        Task<bool> AddToCartAsync(string userId, AddToCartViewModel model);
        Task<bool> UpdateCartItemAsync(string userId, int bookId, int quantity);
        Task<bool> RemoveFromCartAsync(string userId, int bookId);
        Task<bool> ClearCartAsync(string userId);
        Task<int> GetCartItemCountAsync(string userId);
    }

    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartService> _logger;

        public CartService(ApplicationDbContext context, ILogger<CartService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CartViewModel> GetCartAsync(string userId)
        {
            var cartItems = await _context.CartItems
                .Include(ci => ci.Book)
                    .ThenInclude(b => b.Category)
                .Include(ci => ci.Book.Reviews)
                .Where(ci => ci.UserId == userId)
                .OrderBy(ci => ci.CreatedAt)
                .ToListAsync();

            return new CartViewModel
            {
                Items = cartItems.Select(MapToViewModel).ToList()
            };
        }

        public async Task<bool> AddToCartAsync(string userId, AddToCartViewModel model)
        {
            var book = await _context.Books.FindAsync(model.BookId);
            if (book == null || !book.IsActive || book.StockQuantity < model.Quantity)
                return false;

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.BookId == model.BookId);

            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + model.Quantity;
                if (newQuantity > book.StockQuantity || newQuantity > 10)
                    return false;

                existingItem.Quantity = newQuantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = userId,
                    BookId = model.BookId,
                    Quantity = model.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCartItemAsync(string userId, int bookId, int quantity)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Book)
                .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.BookId == bookId);

            if (cartItem == null) return false;

            if (quantity <= 0)
                return await RemoveFromCartAsync(userId, bookId);

            if (quantity > cartItem.Book.StockQuantity || quantity > 10)
                return false;

            cartItem.Quantity = quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromCartAsync(string userId, int bookId)
        {
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.BookId == bookId);

            if (cartItem == null) return false;

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            var cartItems = await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any()) return true;

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCartItemCountAsync(string userId)
        {
            return await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .SumAsync(ci => ci.Quantity);
        }

        private CartItemViewModel MapToViewModel(CartItem cartItem)
        {
            return new CartItemViewModel
            {
                Id = cartItem.Id,
                BookId = cartItem.BookId,
                Quantity = cartItem.Quantity,
                CreatedAt = cartItem.CreatedAt,
                Book = new BookViewModel
                {
                    Id = cartItem.Book.Id,
                    Title = cartItem.Book.Title,
                    Author = cartItem.Book.Author,
                    Price = cartItem.Book.Price,
                    DiscountPrice = cartItem.Book.DiscountPrice,
                    StockQuantity = cartItem.Book.StockQuantity,
                    IsActive = cartItem.Book.IsActive,
                    Category = cartItem.Book.Category,
                    Reviews = cartItem.Book.Reviews
                }
            };
        }
    }
}