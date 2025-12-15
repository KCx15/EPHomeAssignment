using HomeAssignment.Models;
using HomeAssignment.Repositories;
using Microsoft.Extensions.Caching.Memory;


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


    public Task Approve(string id)
    {
        // In-memory repo is used only for temporary storage before DB commit.
        // Approvals are always done on the DB repository, so nothing to do here.
        return Task.CompletedTask;
    }

}
