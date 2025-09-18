using Microsoft.EntityFrameworkCore;
using BookStoreMVC.Data;
using BookStoreMVC.Models.Entities;
using BookStoreMVC.Models.ViewModels;

namespace BookStoreMVC.Services
{
    public interface ICartService
    {
        Task<CartViewModel> GetCartAsync(string userId);
        Task<bool> AddToCartAsync(string userId, int bookId, int quantity = 1);
        Task<bool> UpdateCartItemAsync(string userId, int bookId, int quantity);
        Task<bool> RemoveFromCartAsync(string userId, int bookId);
        Task<bool> ClearCartAsync(string userId);
        Task<int> GetCartItemCountAsync(string userId);
        Task<decimal> GetCartTotalAsync(string userId);
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
            try
            {
                var cartItems = await _context.CartItems
                    .Include(ci => ci.Book)
                        .ThenInclude(b => b.Category)
                    .Where(ci => ci.UserId == userId)
                    .OrderBy(ci => ci.CreatedAt)
                    .ToListAsync();

                var cartViewModel = new CartViewModel
                {
                    Items = cartItems,
                    ShippingCost = CalculateShippingCost(cartItems.Sum(ci => ci.TotalPrice))
                };

                return cartViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart for user: {UserId}", userId);
                return new CartViewModel();
            }
        }

        public async Task<bool> AddToCartAsync(string userId, int bookId, int quantity = 1)
        {
            try
            {
                // Check if book exists and is in stock
                var book = await _context.Books.FindAsync(bookId);
                if (book == null || !book.IsActive || book.StockQuantity < quantity)
                {
                    return false;
                }

                // Check if item already exists in cart
                var existingItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.BookId == bookId);

                if (existingItem != null)
                {
                    // Update quantity if item exists
                    var newQuantity = existingItem.Quantity + quantity;
                    if (newQuantity > book.StockQuantity)
                    {
                        return false; // Not enough stock
                    }

                    existingItem.Quantity = newQuantity;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Create new cart item
                    var cartItem = new CartItem
                    {
                        UserId = userId,
                        BookId = bookId,
                        Quantity = quantity,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.CartItems.Add(cartItem);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Added {Quantity} of book {BookId} to cart for user {UserId}",
                    quantity, bookId, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding book {BookId} to cart for user {UserId}", bookId, userId);
                return false;
            }
        }

        public async Task<bool> UpdateCartItemAsync(string userId, int bookId, int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    return await RemoveFromCartAsync(userId, bookId);
                }

                var cartItem = await _context.CartItems
                    .Include(ci => ci.Book)
                    .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.BookId == bookId);

                if (cartItem == null)
                {
                    return false;
                }

                // Check stock availability
                if (quantity > cartItem.Book.StockQuantity)
                {
                    return false;
                }

                cartItem.Quantity = quantity;
                cartItem.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated cart item quantity to {Quantity} for book {BookId} and user {UserId}",
                    quantity, bookId, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item for book {BookId} and user {UserId}", bookId, userId);
                return false;
            }
        }

        public async Task<bool> RemoveFromCartAsync(string userId, int bookId)
        {
            try
            {
                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.BookId == bookId);

                if (cartItem == null)
                {
                    return false;
                }

                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Removed book {BookId} from cart for user {UserId}", bookId, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing book {BookId} from cart for user {UserId}", bookId, userId);
                return false;
            }
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            try
            {
                var cartItems = await _context.CartItems
                    .Where(ci => ci.UserId == userId)
                    .ToListAsync();

                if (!cartItems.Any())
                {
                    return true;
                }

                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cleared cart for user {UserId}", userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
                return false;
            }
        }

        public async Task<int> GetCartItemCountAsync(string userId)
        {
            try
            {
                return await _context.CartItems
                    .Where(ci => ci.UserId == userId)
                    .SumAsync(ci => ci.Quantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart item count for user {UserId}", userId);
                return 0;
            }
        }

        public async Task<decimal> GetCartTotalAsync(string userId)
        {
            try
            {
                var cartItems = await _context.CartItems
                    .Include(ci => ci.Book)
                    .Where(ci => ci.UserId == userId)
                    .ToListAsync();

                return cartItems.Sum(ci => ci.TotalPrice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart total for user {UserId}", userId);
                return 0;
            }
        }

        private static decimal CalculateShippingCost(decimal subtotal)
        {
            // Free shipping for orders over $50
            if (subtotal >= 50)
                return 0;

            // Standard shipping rate
            return 5.99m;
        }
    }
}