namespace HomeAssignment.Models
{

    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Collections.Generic;

    public class Restaurant : IItemValidating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string OwnerEmailAddress { get; set; }

        [Required]
        public ItemStatus Status { get; set; } = ItemStatus.Pending;

        public ICollection<MenuItem> MenuItems { get; set; }

        public List<string> GetValidators()
        {
            return new List<string> { "SITE_ADMIN" };
        }

        public string GetCardPartial() => "_RestaurantCard";
    }
}