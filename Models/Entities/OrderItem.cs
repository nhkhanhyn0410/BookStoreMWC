using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreMVC.Models.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

        public int BookId { get; set; }
        public virtual Book Book { get; set; } = null!;

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        // Book information at time of order (for historical accuracy)
        [StringLength(200)]
        public string BookTitle { get; set; } = string.Empty;

        [StringLength(100)]
        public string BookAuthor { get; set; } = string.Empty;

        [StringLength(500)]
        public string? BookImageUrl { get; set; }
    }
}
