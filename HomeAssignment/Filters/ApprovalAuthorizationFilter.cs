using HomeAssignment.Models;
using HomeAssignment.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HomeAssignment.Filters
{
    public class ApprovalAuthorizationFilter : IAsyncActionFilter
    {
        private readonly IItemsRepository _db;

        public ApprovalAuthorizationFilter([FromKeyedServices("db")] IItemsRepository db)
        {
            _db = db;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var email = context.HttpContext.Session.GetString("UserEmail");
            if (email == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            if (!context.ActionArguments.TryGetValue("id", out var idObj))
            {
                context.Result = new BadRequestResult();
                return;
            }

            var items = await _db.GetAsync();
            var item = items.FirstOrDefault(i =>
                (i is Restaurant r && r.Id == (int)idObj) ||
                (i is MenuItem m && m.Id.ToString() == idObj.ToString())
            );

            if (item == null)
            {
                context.Result = new NotFoundResult();
                return;
            }

            if (!item.GetValidators().Contains(email))
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}
