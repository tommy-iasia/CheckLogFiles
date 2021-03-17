using CheckLogServer.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace CheckLogServer.Filter
{
    public class LoginFilter : IAsyncPageFilter
    {
        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var login = context.HttpContext.RequestServices.GetRequiredService<Login>();
            if (login.Success)
            {
                await next.Invoke();
            }
            else
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
