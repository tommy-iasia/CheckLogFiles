using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckLogServer.Middlewares;
using CheckLogServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CheckLogServer.Pages.Account
{
    public class LogoutModel : PageModel
    {
        public LogoutModel(AccountsSaver accountsSaver, Login login)
        {
            this.accountsSaver = accountsSaver;
            this.login = login;
        }
        private readonly AccountsSaver accountsSaver;
        private readonly Login login;

        public async Task<IActionResult> OnGetAsync()
        {
            if (login.Success)
            {
                login.Account.Sessions = login.Account.Sessions
                   .Except(new[] { login.Session })
                   .ToArray();

                await accountsSaver.SaveAsync();
            }

            return RedirectToPage("/Index");
        }
    }
}
