using HomeAssignment.Models;
using System.Text.Json.Nodes;

namespace HomeAssignment.Factories
{
    public class ImportItemFactory
    {
        public List<IItemValidating> Create(string json)
        {
            var items = new List<IItemValidating>();
            var data = JsonNode.Parse(json).AsArray();

            foreach (var node in data)
            {
                string type = node["Type"]?.ToString();

                switch (type)
                {
                    case "Restaurant":
                        items.Add(new Restaurant
                        {
                            Name = node["Name"]?.ToString(),
                            OwnerEmailAddress = node["OwnerEmailAddress"]?.ToString(),
                            Status = ItemStatus.Pending
                        });
                        break;

                    case "MenuItem":
                        items.Add(new MenuItem
                        {
                            Title = node["Title"]?.ToString(),
                            Price = decimal.Parse(node["Price"]?.ToString() ?? "0"),
                            RestaurantId = int.Parse(node["RestaurantId"]?.ToString() ?? "0"),
                            Status = ItemStatus.Pending
                        });
                        break;
                }
            }

            return items;
        }
    }
}
