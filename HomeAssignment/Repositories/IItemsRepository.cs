using HomeAssignment.Models;

namespace HomeAssignment.Repositories
{
    public interface IItemsRepository
    {
        Task SaveAsync(IEnumerable<IItemValidating> items);
        Task<IEnumerable<IItemValidating>> GetAsync();

        Task Approve(int id);
    }
}
