using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeAssignment.Models
{
    public class MenuItem : IItemValidating
    {
        // DB PRIMARY KEY (assignment requires GUID)
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // JSON id like "M-2001"
        [Required, MaxLength(50)]
        public string ExternalId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
        [MaxLength(10)]
        public string? Currency { get; set; }


        // JSON restaurantId like "R-1001"
        // Used ONLY during import for mapping
        [NotMapped]
        public string RestaurantExternalId { get; set; }

        // DB FK → Restaurants.Id (set during SaveAsync remapping)
        [Required]
        public int RestaurantId { get; set; }

        public string? ImageUrl { get; set; }

        // Navigation property
        public Restaurant? Restaurant { get; set; }

        [Required]
        public ItemStatus Status { get; set; } = ItemStatus.Pending;

        // MenuItems must be approved by restaurant owner
        public List<string> GetValidators()
        {
            if (Restaurant != null)
                return new List<string> { Restaurant.OwnerEmailAddress };

            return new List<string>();
        }

        public string GetCardPartial() => "_MenuItemCard";
    }
}
