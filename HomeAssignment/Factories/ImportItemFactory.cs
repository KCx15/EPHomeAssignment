using HomeAssignment.Models;
using System.Globalization;
using System.Text.Json.Nodes;

namespace HomeAssignment.Factories
{
    public class ImportItemFactory
    {
        public List<IItemValidating> Create(string json)
        {
            var items = new List<IItemValidating>();

            var data = JsonNode.Parse(json)?.AsArray();
            if (data == null) return items;

            foreach (var node in data)
            {
                if (node is not JsonObject obj) continue;

                string? GetString(string key)
                {
                    foreach (var kv in obj)
                    {
                        var k = (kv.Key ?? "").Trim();
                        if (k.Equals(key, StringComparison.OrdinalIgnoreCase))
                            return kv.Value?.ToString();
                    }
                    return null;
                }

                var type = GetString("type");
                if (type == null) continue;

                if (type.Equals("restaurant", StringComparison.OrdinalIgnoreCase))
                {
                    var externalId = GetString("id");

                    items.Add(new Restaurant
                    {
                        ExternalId = externalId ?? Guid.NewGuid().ToString(),
                        Name = GetString("name") ?? "(Unnamed Restaurant)",
                        Description = GetString("description"),
                        Address = GetString("address"),
                        Phone = GetString("phone"),
                        OwnerEmailAddress = GetString("ownerEmailAddress") ?? "",
                        Status = ItemStatus.Pending
                    });
                }
                else if (type.Equals("menuItem", StringComparison.OrdinalIgnoreCase))
                {
                    var externalId = GetString("id");
                    var restaurantExternalId = GetString("restaurantId");

                    decimal price = 0m;
                    var priceStr = GetString("price");
                    if (!string.IsNullOrWhiteSpace(priceStr))
                        decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out price);

                    items.Add(new MenuItem
                    {
                        ExternalId = externalId ?? Guid.NewGuid().ToString(),
                        Title = GetString("title") ?? "(Untitled)",
                        Price = price,
                        Currency = GetString("currency"),          // ✅ ADD THIS

                        RestaurantExternalId = restaurantExternalId ?? "",
                        RestaurantId = 0,

                        Status = ItemStatus.Pending
                    });
                }
            }

            return items;
        }
    }
}
