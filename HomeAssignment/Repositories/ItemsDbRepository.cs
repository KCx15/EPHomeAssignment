using Microsoft.EntityFrameworkCore;
using HomeAssignment.Data;
using HomeAssignment.Models;


namespace HomeAssignment.Repositories
{
    public class ItemsDbRepository : IItemsRepository
    {
        private readonly AppDbContext _ctx;   // <-- Declare this

        public ItemsDbRepository(AppDbContext ctx)
        {
            _ctx = ctx;                       // <-- Assign in constructor
        }

        public async Task SaveAsync(IEnumerable<IItemValidating> items)
        {
            foreach (var item in items)
            {
                if (item is Restaurant r)
                    _ctx.Restaurants.Add(r);
                else if (item is MenuItem m)
                    _ctx.MenuItems.Add(m);
            }

            await _ctx.SaveChangesAsync();
        }

        public async Task<IEnumerable<IItemValidating>> GetAsync()
        {
            var restaurants = await _ctx.Restaurants.ToListAsync();
            var menuItems = await _ctx.MenuItems.ToListAsync();

            var all = new List<IItemValidating>();
            all.AddRange(restaurants);
            all.AddRange(menuItems);
            return all;
        }

        public async Task Approve(int id)
        {
            var restaurant = await _ctx.Restaurants.FindAsync(id);
            if (restaurant != null)
            {
                restaurant.Status = ItemStatus.Approved;
                await _ctx.SaveChangesAsync();
                return;
            }

            var menuItem = await _ctx.MenuItems.FindAsync(id);
            if (menuItem != null)
            {
                menuItem.Status = ItemStatus.Approved;
                await _ctx.SaveChangesAsync();
            }
        }
    }
}

