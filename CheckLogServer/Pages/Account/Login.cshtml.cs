using System;
using System.Linq;
using System.Threading.Tasks;
using CheckLogServer.Middlewares;
using CheckLogServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CheckLogServer.Pages.Account
{
    public class LoginModel : PageModel
    {
        public LoginModel(AccountsSaver accountsSaver, Login login)
        {
            this.accountsSaver = accountsSaver;
            this.login = login;
        }
        private readonly AccountsSaver accountsSaver;
        private readonly Login login;

        [BindProperty]
        public Models.Account Account { get; set; }

        public IActionResult OnGet()
        {
            if (login.Success)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var accounts = await accountsSaver.LoadAsync();

            var account = accounts.FirstOrDefault(t => t.Username == Account.Username);
            if (account == null)
            {
                ModelState.AddModelError(nameof(Account.Username), "account not found");
                return Page();
            }

            if (account.Password != Account.Password)
            {
                ModelState.AddModelError(nameof(Account.Password), "password incorrect");
                return Page();
            }

            var session = LoginMiddleware.AddSession(Response);
            account.Sessions = (account.Sessions ?? Array.Empty<string>())
                .Append(session).ToArray();

            await accountsSaver.SaveAsync();

            return RedirectToPage("/Index");
        }
    }
}
