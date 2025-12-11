using HomeAssignment.Models;
using HomeAssignment.Repositories;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ItemsInMemoryRepository : IItemsRepository
{
    private readonly IMemoryCache _cache;
    private const string CacheKey = "InMemoryItems";

    public ItemsInMemoryRepository(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<IEnumerable<IItemValidating>> GetAsync()
    {
        _cache.TryGetValue(CacheKey, out IEnumerable<IItemValidating> items);
        return Task.FromResult(items ?? Enumerable.Empty<IItemValidating>());
    }

    public Task SaveAsync(IEnumerable<IItemValidating> items)
    {
        _cache.Set(CacheKey, items);
        return Task.CompletedTask;
    }

    public void Clear()
    {
        _cache.Remove(CacheKey);
    }

    
    public Task Approve(int id)
    {
        _cache.TryGetValue(CacheKey, out IEnumerable<IItemValidating> items);
        if (items == null) return Task.CompletedTask;

        foreach (var item in items)
        {
            if (item is Restaurant r && r.Id == id)
                r.Status = ItemStatus.Approved;
            else if (item is MenuItem m && m.Id.GetHashCode() == id)
                m.Status = ItemStatus.Approved;
        }

        _cache.Set(CacheKey, items);
        return Task.CompletedTask;
    }
}
