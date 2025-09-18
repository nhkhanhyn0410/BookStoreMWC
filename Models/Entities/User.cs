using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BookStoreMVC.Models.Entities
{
    public class User : IdentityUser
    {

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(500)]
        public string? ProfileImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        //Navigation properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<WishListItem> WishListItems { get; set; } = new List<WishListItem>();

        //Computed properties
        public string FullName => $"{FirstName} {LastName}";

        public string DisplayName => !string.IsNullOrEmpty(FirstName) ? FirstName : UserName ?? "User";

        public int Age => DateOfBirth.HasValue ?
            DateTime.Now.Year - DateOfBirth.Value.Year -
            (DateTime.Now.DayOfYear < DateOfBirth.Value.DayOfYear ? 1 : 0) : 0;

    }
}