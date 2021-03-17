using CheckLogServer.Models;
using CheckLogWorker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CheckLogServer.Middlewares
{
    public class LoginMiddleware
    {
        public LoginMiddleware(RequestDelegate next) => this.next = next;
        private readonly RequestDelegate next;

        public async Task InvokeAsync(HttpContext context)
        {
            var session = context.Request.Cookies[SessionKey];
            if (session != null)
            {
                var login = context.RequestServices.GetRequiredService<Login>();
                login.Session = session;

                var accountsSaver = context.RequestServices.GetRequiredService<AccountsSaver>();
                var accounts = await accountsSaver.LoadAsync();

                login.Account = accounts?.FirstOrDefault(t => t.Sessions?.Contains(session) ?? false);
            }

            next?.Invoke(context);
        }

        private const string SessionKey = "Session";
        public static string AddSession(HttpResponse response)
        {
            var guid = Guid.NewGuid().ToString();

            response.Cookies.Append(SessionKey, guid);

            return guid;
        }
    }
}
