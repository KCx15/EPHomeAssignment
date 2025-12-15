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
            _adminEmail = config["SiteAdminEmail"];
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var email = context.HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email))
            {
                context.Result = new ForbidResult();
                return;
            }

            // Try to get "id" from action arguments
            if (!context.ActionArguments.TryGetValue("id", out var idObj) || idObj == null)
            {
                // Fallback: try route data (e.g. /Verification/Approve/{id})
                idObj = context.RouteData.Values["id"];

                if (idObj == null)
                {
                    context.Result = new BadRequestObjectResult("Missing ID");
                    return;
                }
            }

            var idStr = idObj.ToString();

            var items = await _db.GetAsync();
            IItemValidating item = null;

            // Try Restaurant (int)
            if (int.TryParse(idStr, out var restId))
            {
                item = items.OfType<Restaurant>().FirstOrDefault(r => r.Id == restId);
            }

            // Try MenuItem (Guid)
            if (item == null && Guid.TryParse(idStr, out var guid))
            {
                item = items.OfType<MenuItem>().FirstOrDefault(m => m.Id == guid);
            }

            if (item == null)
            {
                context.Result = new NotFoundObjectResult("Item not found");
                return;
            }

            bool isAdmin = email.Equals(_adminEmail, StringComparison.OrdinalIgnoreCase);

            if (isAdmin && item is Restaurant)
            {
                await next();
                return;
            }

            var allowed = item.GetValidators();

            if (!allowed.Contains(email, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }

    }
}
