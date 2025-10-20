using BookStoreMVC.Models.ViewModels;
using System.Text.Json;

namespace BookStoreMVC.Services
{
    public interface ISessionCartService
    {
        CartViewModel GetCart();
        void AddToCart(int bookId, int quantity);
        void UpdateCartItem(int bookId, int quantity);
        void RemoveFromCart(int bookId);
        void ClearCart();
        int GetCartItemCount();
    }

    public class SessionCartService : ISessionCartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBookService _bookService;
        private const string CartSessionKey = "GuestCart";

        public SessionCartService(IHttpContextAccessor httpContextAccessor, IBookService bookService)
        {
            _httpContextAccessor = httpContextAccessor;
            _bookService = bookService;
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
            catch
            {
                return new CartViewModel();
            }
        }

        public void AddToCart(int bookId, int quantity)
        {
            var cart = GetCart();

            var book = _bookService.GetBookByIdAsync(bookId).Result;
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