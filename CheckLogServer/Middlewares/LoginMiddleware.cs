using CheckLogServer.Models;
using CheckLogWorker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var code = context.Request.Cookies[SessionKey];
            if (code != null)
            {
                var database = context.RequestServices.GetRequiredService<DatabaseContext>();
                var session = await database.AccountSessions
                    .Where(t => t.Code == code)
                    .Include(t => t.Account)
                    .FirstOrDefaultAsync();

                if (session != null)
                {
                    var login = context.RequestServices.GetRequiredService<Login>();
                    login.Session = session;
                    login.Account = session.Account;
                }
            }

            await next?.Invoke(context);
        }

        private const string SessionKey = "Session";
        public static AccountSession AddSession(Account account, DatabaseContext database, HttpResponse response)
        {
            var code = Guid.NewGuid().ToString();

            var accountSession = new AccountSession
            {
                Account = account,
                Code = code,
                StartTime = DateTime.Now
            };

            database.AccountSessions.Add(accountSession);

            response.Cookies.Append(SessionKey, code);

            return accountSession;
        }
    }
}
