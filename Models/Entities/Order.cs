using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace BookStoreMVC.Models.Entities
{
    public enum OrderStatus
    {
        Pending = 1,
        Processing = 2,
        Shipped = 3,
        Delivered = 4,
        Cancelled = 5,
        Refunded = 6
    }

    public enum PaymentStatus
    {
        Pending = 1,
        Paid = 2,
        Failed = 3,
        Refunded = 4
    }

    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual User User { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string OrderNumber { get; set; } = string.Empty;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal ShippingCost { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Tax { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Discount { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Total { get; set; }

        // Shipping Infomation
        [Required]
        [StringLength(100)]
        public string ShippingFirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ShippingLastName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ShippingCity { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ShippingPostalCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ShippingCountry { get; set; } = string.Empty;

        [StringLength(20)]
        public string? ShippingPhone { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? TrackingNumber { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }

        // Naviagation properties
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Computed propeties
        [NotMapped]
        public string ShippingFullName => $"{ShippingFirstName} {ShippingLastName}";

        [NotMapped]
        public string ShippinFullAddress => $"{ShippingAddress}, {ShippingCity}, {ShippingPostalCode}, {ShippingCountry}";

        [NotMapped]
        public int TotalItems => OrderItems.Sum(oi => oi.Quantity);

        [NotMapped]
        public string StatusDisplay => Status switch
        {
            OrderStatus.Pending => "Pending",
            OrderStatus.Processing => "Processing",
            OrderStatus.Shipped => "Shipped",
            OrderStatus.Delivered => "Delivered",
            OrderStatus.Cancelled => "Cancelled",
            OrderStatus.Refunded => "Refunded",
            _ => "Unknown"
        };

        [NotMapped]
        public string StatusColor => Status switch
        {
            OrderStatus.Pending => "yellow",
            OrderStatus.Processing => "blue",
            OrderStatus.Shipped => "purple",
            OrderStatus.Delivered => "green",
            OrderStatus.Cancelled => "red",
            OrderStatus.Refunded => "gray",
            _ => "gray"
        };

    }
}