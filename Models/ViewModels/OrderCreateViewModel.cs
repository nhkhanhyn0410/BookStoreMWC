using System.ComponentModel.DataAnnotations;

namespace BookStoreMVC.Models.ViewModels
{
    public class OrderCreateViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string ShippingFirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string ShippingLastName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Address")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "City")]
        public string ShippingCity { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Postal Code")]
        public string ShippingPostalCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Country")]
        public string ShippingCountry { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? ShippingPhone { get; set; }

        [StringLength(500)]
        [Display(Name = "Order Notes")]
        public string? Notes { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = string.Empty;

        public CartViewModel Cart { get; set; } = new();

        public bool UseProfileAddress { get; set; }
    }
}