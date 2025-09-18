using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreMVC.Models.Entities
{
    public class WishListItem
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public virtual User User { get; set; } = null!;

        public int BookId { get; set; }
        public virtual Book Book { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}