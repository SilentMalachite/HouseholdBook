using System.ComponentModel.DataAnnotations;

namespace HouseholdBook.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = "Expense"; // Income or Expense
    }
}