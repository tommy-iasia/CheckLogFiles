using System.Linq;
using System.Threading.Tasks;
using CheckLogServer.Middlewares;
using CheckLogServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CheckLogServer.Pages.Account
{
    public class LoginModel : PageModel
    {
        public LoginModel(DatabaseContext database, Login login)
        {
            this.database = database;
            this.login = login;
        }
        private readonly DatabaseContext database;
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

            var account = await database.Accounts.FirstOrDefaultAsync(t => t.Username == Account.Username);
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

            var _ = LoginMiddleware.AddSession(account, database, Response);
            await database.SaveChangesAsync();

            return RedirectToPage("/Index");
        }
    }
}
