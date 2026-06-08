using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using WarehouseAccessAPI.Common;
using WarehouseAccessAPI.Models;

namespace WarehouseAccessAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class MasterDataAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            
            // Bypass authorization for Excel template download endpoints
            var actionName = context.ActionDescriptor.RouteValues["action"];
            if (actionName != null && actionName.Contains("Template", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            var userId = httpContext.Request.Headers["X-User-Id"].ToString()?.Trim();
            var factoryId = httpContext.Request.Headers["X-Factory-Id"].ToString()?.Trim();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(factoryId))
            {
                context.Result = new ObjectResult(new Response<object>(false, null, "Unauthorized: Missing identity headers (X-User-Id or X-Factory-Id)"))
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            var db = httpContext.RequestServices.GetRequiredService<FgamContext>();
            var account = await db.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.EmployeeId == userId && a.FactoryId == factoryId);

            if (account == null)
            {
                context.Result = new ObjectResult(new Response<object>(false, null, "Unauthorized: Account not found"))
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            if (account.RecordStatus != 2)
            {
                context.Result = new ObjectResult(new Response<object>(false, null, "Forbidden: Only users with RecordStatus = 2 can operate Master Data"))
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            await next();
        }
    }
}
