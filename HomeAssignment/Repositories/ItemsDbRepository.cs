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


        public async Task SaveAsync(IEnumerable<IItemValidating> items)
        {
            var restaurants = new List<Restaurant>();
            var menuItems = new List<MenuItem>();

           
            foreach (var item in items)
            {
                if (item is Restaurant r)
                {
                    r.Id = 0; 
                    restaurants.Add(r);
                }
                else if (item is MenuItem m)
                {
                    menuItems.Add(m);
                }
            }

            
            _ctx.Restaurants.AddRange(restaurants);
            await _ctx.SaveChangesAsync();

          
            var idMap = restaurants.ToDictionary(
                r => r.ExternalId.Trim(),
                r => r.Id,
                StringComparer.OrdinalIgnoreCase
            );

          
            foreach (var m in menuItems)
            {
                var key = (m.RestaurantExternalId ?? "").Trim();

                if (string.IsNullOrWhiteSpace(key))
                    throw new Exception($"MenuItem '{m.ExternalId}' has missing restaurantId in JSON.");

                if (!idMap.TryGetValue(key, out var newDbRestaurantId))
                    throw new Exception($"MenuItem '{m.ExternalId}' refers to restaurantId '{key}', but no matching restaurant exists in JSON.");

                m.RestaurantId = newDbRestaurantId; 
            }

            _ctx.MenuItems.AddRange(menuItems);
            await _ctx.SaveChangesAsync();
        }



        public async Task<IEnumerable<IItemValidating>> GetAsync()
        {
            var restaurants = await _ctx.Restaurants.ToListAsync();
            var menuItems = await _ctx.MenuItems.ToListAsync();

            var list = new List<IItemValidating>();
            list.AddRange(restaurants);
            list.AddRange(menuItems);

            return list;
        }

       
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

