using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeAssignment.Models
{
    public class Restaurant : IItemValidating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // JSON id like "R-1001" (stored in DB for mapping + integrity)
        [Required, MaxLength(50)]
        public string ExternalId { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [Required, EmailAddress, MaxLength(254)]
        public string OwnerEmailAddress { get; set; }

        public string? ImageUrl { get; set; }

        [Required]
        public ItemStatus Status { get; set; } = ItemStatus.Pending;

        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

        public List<string> GetValidators()
        {
           
            return new List<string> { "SITE_ADMIN" };
        }

        public string GetCardPartial() => "_RestaurantCard";
    }
}
