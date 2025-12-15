using System.Collections.Generic;

namespace HomeAssignment.Models
{
    /// <summary>
    /// Common contract for anything that can appear in the catalog
    /// and be approved (Restaurant, MenuItem, etc.).
    /// </summary>
    public interface IItemValidating
    {
        /// <summary>
        /// Returns the list of email addresses that are allowed to approve this item.
        /// For Restaurants: site admin.
        /// For MenuItems: restaurant owner.
        /// </summary>
        List<string> GetValidators();

        /// <summary>
        /// Returns the name of the partial view used to render this item in the catalog.
        /// e.g. "_RestaurantCard" or "_MenuItemCard".
        /// </summary>
        string GetCardPartial();
    }
}
