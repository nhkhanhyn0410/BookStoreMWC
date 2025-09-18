using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreMVC.Models.Entities
{
    public class Review
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public virtual User User { get; set; } = null!;

        public int BookId { get; set; }
        public virtual Book Book { get; set; } = null!;

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(100)]
        public string? Title { get; set; }

        [Column(TypeName = "text")]
        public string? Comment { get; set; }

        public bool IsVerifiedPurchase { get; set; }

        public bool IsApproved { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Computed properties
        [NotMapped]
        public string RatingStars => new string('★', Rating) + new string('☆', 5 - Rating);

        [NotMapped]
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.UtcNow - CreatedAt;
                return timeSpan switch
                {
                    { TotalDays: >= 365 } => $"{(int)(timeSpan.TotalDays / 365)} year(s) ago",
                    { TotalDays: >= 30 } => $"{(int)(timeSpan.TotalDays / 30)} month(s) ago",
                    { TotalDays: >= 1 } => $"{(int)timeSpan.TotalDays} day(s) ago",
                    { TotalHours: >= 1 } => $"{(int)timeSpan.TotalHours} hour(s) ago",
                    { TotalMinutes: >= 1 } => $"{(int)timeSpan.TotalMinutes} minute(s) ago",
                    _ => "Just now"
                };
            }
        }
    }
}