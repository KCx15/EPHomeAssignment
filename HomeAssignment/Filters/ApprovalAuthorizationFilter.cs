using HomeAssignment.Models;
using HomeAssignment.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HomeAssignment.Filters
{
    public class ApprovalAuthorizationFilter : IAsyncActionFilter
    {
        private readonly IItemsRepository _db;
        private readonly string _adminEmail;

        public ApprovalAuthorizationFilter(
            [FromKeyedServices("db")] IItemsRepository db,
            IConfiguration config)
        {
            _db = db;
            _adminEmail = config["SiteAdminEmail"] ?? "";
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var email = context.HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                context.Result = new ForbidResult();
                return;
            }

            // ✅ Support both single "id" and multi "ids"
            List<string> ids = new();

            if (context.ActionArguments.TryGetValue("ids", out var idsObj) && idsObj is List<string> listIds)
            {
                ids = listIds;
            }
            else if (context.ActionArguments.TryGetValue("id", out var idObj) && idObj != null)
            {
                ids = new List<string> { idObj.ToString()! };
            }
            else
            {
                // Fallback: route-based id
                var routeId = context.RouteData.Values["id"]?.ToString();
                if (!string.IsNullOrWhiteSpace(routeId))
                    ids = new List<string> { routeId };
            }

            if (ids.Count == 0)
            {
                context.Result = new BadRequestObjectResult("Missing ID(s)");
                return;
            }

            var items = (await _db.GetAsync()).ToList();
            bool isAdmin = email.Equals(_adminEmail, StringComparison.OrdinalIgnoreCase);

            // ✅ Validate EVERY selected id
            foreach (var idStr in ids)
            {
                if (string.IsNullOrWhiteSpace(idStr))
                {
                    context.Result = new BadRequestObjectResult("Invalid ID");
                    return;
                }

                IItemValidating? item = null;

                // Restaurant: int id
                if (int.TryParse(idStr, out var restId))
                    item = items.OfType<Restaurant>().FirstOrDefault(r => r.Id == restId);

                // MenuItem: Guid id
                if (item == null && Guid.TryParse(idStr, out var guid))
                    item = items.OfType<MenuItem>().FirstOrDefault(m => m.Id == guid);

                if (item == null)
                {
                    context.Result = new NotFoundObjectResult($"Item not found: {idStr}");
                    return;
                }

                // Admin can approve restaurants
                if (isAdmin && item is Restaurant)
                    continue;

                var allowed = item.GetValidators();
                if (!allowed.Contains(email, StringComparer.OrdinalIgnoreCase))
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }

            await next();
        }
    }
}
