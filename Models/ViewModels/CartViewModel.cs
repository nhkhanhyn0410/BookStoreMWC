using System.ComponentModel.DataAnnotations;
using BookStoreMVC.Models.Entities;

namespace BookStoreMVC.Models.ViewModels
{
    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new();

        public decimal SubTotal => Items.Sum(item => item.TotalPrice);
        public decimal ShippingCost { get; set; } = 0;
        public decimal Tax => SubTotal * 0.1m; // thuế có thể sửa
        public decimal Total => SubTotal + ShippingCost + Tax;

        public int TotalItems => Items.Sum(item => item.Quantity);

        public bool IsEmpty => !Items.Any();

        public string? PromoCode { get; set; }
        public decimal PromoDiscount { get; set; }
    }
}