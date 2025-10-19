using System.ComponentModel.DataAnnotations;

namespace HouseholdBook.Models
{
    public class AutoCategorizationRule
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Keyword { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public int? Priority { get; set; }

        public int MatchCount { get; set; }
    }
}
