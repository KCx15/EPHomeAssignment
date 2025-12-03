using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeAssignment.Models
{
    public class MenuItem : IItemValidating
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public int RestaurantId { get; set; }

        public Restaurant Restaurant { get; set; }

        [Required]
        public ItemStatus Status { get; set; } = ItemStatus.Pending;

        public List<string> GetValidators()
        {
            if (Restaurant != null)
                return new List<string> { Restaurant.OwnerEmailAddress };

            return new List<string>();
        }

        public string GetCardPartial() => "_MenuItemCard";
    }
}
