using Microsoft.EntityFrameworkCore;
using HomeAssignment.Data;
using HomeAssignment.Models;

namespace HomeAssignment.Repositories
{
    public class ItemsDbRepository : IItemsRepository
    {
        private readonly AppDbContext _ctx;

        public ItemsDbRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        // --------------------------------------------------------------
        // SAVE LOGIC — Restaurants first → remap IDs → save MenuItems
        // --------------------------------------------------------------
        public async Task SaveAsync(IEnumerable<IItemValidating> items)
        {
            var restaurants = new List<Restaurant>();
            var menuItems = new List<MenuItem>();

            // Split items
            foreach (var item in items)
            {
                if (item is Restaurant r)
                {
                    r.Id = 0; // force identity insert
                    restaurants.Add(r);
                }
                else if (item is MenuItem m)
                {
                    menuItems.Add(m);
                }
            }

            // 1) Save restaurants first so DB generates int Id
            _ctx.Restaurants.AddRange(restaurants);
            await _ctx.SaveChangesAsync();

            // 2) Map Restaurant.ExternalId ("R-1001") -> Restaurant.Id (int)
            var idMap = restaurants.ToDictionary(
                r => r.ExternalId.Trim(),
                r => r.Id,
                StringComparer.OrdinalIgnoreCase
            );

            // 3) Remap menu items restaurant FK
            foreach (var m in menuItems)
            {
                var key = (m.RestaurantExternalId ?? "").Trim();

                if (string.IsNullOrWhiteSpace(key))
                    throw new Exception($"MenuItem '{m.ExternalId}' has missing restaurantId in JSON.");

                if (!idMap.TryGetValue(key, out var newDbRestaurantId))
                    throw new Exception($"MenuItem '{m.ExternalId}' refers to restaurantId '{key}', but no matching restaurant exists in JSON.");

                m.RestaurantId = newDbRestaurantId; // set FK
                                                    // Optional: clear import-only field
                                                    // m.RestaurantExternalId = null; // can't if it's not nullable. leave it.
            }

            // 4) Save menu items
            _ctx.MenuItems.AddRange(menuItems);
            await _ctx.SaveChangesAsync();
        }


        // --------------------------------------------------------------
        // RETURN ALL ITEMS (Restaurants + MenuItems)
        // --------------------------------------------------------------
        public async Task<IEnumerable<IItemValidating>> GetAsync()
        {
            var restaurants = await _ctx.Restaurants.ToListAsync();
            var menuItems = await _ctx.MenuItems.ToListAsync();

            var list = new List<IItemValidating>();
            list.AddRange(restaurants);
            list.AddRange(menuItems);

            return list;
        }

        // --------------------------------------------------------------
        // APPROVE AN ITEM
        // --------------------------------------------------------------
        public async Task Approve(string id)
        {
            if (int.TryParse(id, out var restId))
            {
                var restaurant = await _ctx.Restaurants.FindAsync(restId);
                if (restaurant != null)
                {
                    restaurant.Status = ItemStatus.Approved;
                    await _ctx.SaveChangesAsync();
                    return;
                }
            }

            // MenuItems use GUID for Id — so check them separately
            if (Guid.TryParse(id, out var guid))
            {
                var menuItem = await _ctx.MenuItems.FindAsync(guid);
                if (menuItem != null)
                {
                    menuItem.Status = ItemStatus.Approved;
                    await _ctx.SaveChangesAsync();
                    return;
                }
            }
        }
    }
}

