using BookStoreMVC.Models.ViewModels;
using System.Text.Json;

namespace BookStoreMVC.Services
{
    public interface ISessionCartService
    {
        CartViewModel GetCart();
        void AddToCart(int bookId, int quantity);
        Task AddToCartAsync(int bookId, int quantity);
        void UpdateCartItem(int bookId, int quantity);
        void RemoveFromCart(int bookId);
        void ClearCart();
        int GetCartItemCount();
    }

    public class SessionCartService : ISessionCartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBookService _bookService;
        private readonly ILogger<SessionCartService> _logger;
        private const string CartSessionKey = "GuestCart";

        public SessionCartService(
            IHttpContextAccessor httpContextAccessor,
            IBookService bookService,
            ILogger<SessionCartService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _bookService = bookService;
            _logger = logger;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public CartViewModel GetCart()
        {
            var cartJson = Session.GetString(CartSessionKey);

            if (string.IsNullOrEmpty(cartJson))
            {
                return new CartViewModel();
            }

            try
            {
                var cart = JsonSerializer.Deserialize<CartViewModel>(cartJson);
                return cart ?? new CartViewModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize cart from session. Returning empty cart.");
                return new CartViewModel();
            }
        }

        // DEPRECATED: Use AddToCartAsync instead to avoid blocking async calls
        public void AddToCart(int bookId, int quantity)
        {
            // Using GetAwaiter().GetResult() instead of .Result to reduce deadlock risk
            // However, prefer using AddToCartAsync for better performance
            AddToCartAsync(bookId, quantity).GetAwaiter().GetResult();
        }

        public async Task AddToCartAsync(int bookId, int quantity)
        {
            var cart = GetCart();

            var book = await _bookService.GetBookByIdAsync(bookId);
            if (book == null) return;

            var existingItem = cart.Items.FirstOrDefault(i => i.BookId == bookId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItemViewModel
                {
                    Id = null, // ← Session cart KHÔNG có ID
                    BookId = book.Id,
                    BookTitle = book.Title,
                    BookAuthor = book.Author,
                    BookImage = book.ImageUrl,
                    Price = book.DisplayPrice,
                    Quantity = quantity,
                    CreatedAt = DateTime.UtcNow
                });
            }

            SaveCart(cart);
        }

        public void UpdateCartItem(int bookId, int quantity)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.BookId == bookId);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Items.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }

                SaveCart(cart);
            }
        }

        public void RemoveFromCart(int bookId)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.BookId == bookId);

            if (item != null)
            {
                cart.Items.Remove(item);
                SaveCart(cart);
            }
        }

        public void ClearCart()
        {
            Session.Remove(CartSessionKey);
        }

        public int GetCartItemCount()
        {
            var cart = GetCart();
            return cart.ItemCount;
        }

        private void SaveCart(CartViewModel cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            Session.SetString(CartSessionKey, cartJson);
        }

        // Phương thức để chuyển giỏ hàng session sang database khi user đăng nhập
        public List<CartItemViewModel> GetCartItemsForMigration()
        {
            var cart = GetCart();
            return cart.Items.ToList();
        }
    }
}