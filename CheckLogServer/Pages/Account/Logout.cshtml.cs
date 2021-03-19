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
        public LogoutModel(DatabaseContext database, Login login)
        {
            this.database = database;
            this.login = login;
        }
        private readonly DatabaseContext database;
        private readonly Login login;

        public async Task<IActionResult> OnGetAsync()
        {
            if (login.Success)
            {
                database.AccountSessions.Remove(login.Session);
                await database.SaveChangesAsync();
            }

            return RedirectToPage("/Index");
        }
    }
}
