using BookStoreMVC.Models.Entities;

namespace BookStoreMVC.Models.ViewModels
{
    public class CartViewModel
    {
        public ICollection<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();

        // Calculated properties
        public decimal SubTotal => Items.Sum(i => i.TotalPrice);
        public decimal Tax => SubTotal * 0.1m; // 10% tax
        public decimal ShippingCost => SubTotal >= 50 ? 0 : 5.99m; // Free shipping over $50
        public decimal Total => SubTotal + Tax + ShippingCost;
        public int ItemCount => Items.Sum(i => i.Quantity);
        public bool IsEmpty => !Items.Any();
        public bool QualifiesForFreeShipping => SubTotal >= 50;
        public decimal AmountForFreeShipping => Math.Max(0, 50 - SubTotal);
    }

    public class CartItemViewModel
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }

        // Book information
        public BookViewModel Book { get; set; } = new();

        // Calculated properties
        public decimal UnitPrice => Book.DisplayPrice;
        public decimal TotalPrice => UnitPrice * Quantity;
        public bool InStock => Book.InStock && Book.StockQuantity >= Quantity;
        public int MaxQuantity => Math.Min(Book.StockQuantity, 10); // Limit to 10 per item
    }

    public class AddToCartViewModel
    {
        public int BookId { get; set; }
        public int Quantity { get; set; } = 1;
        public bool IsWishlistItem { get; set; }
    }
}