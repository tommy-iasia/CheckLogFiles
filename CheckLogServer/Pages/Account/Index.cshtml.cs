using System.Collections.Generic;
using System.Threading.Tasks;
using CheckLogServer.Filter;
using CheckLogServer.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CheckLogServer.Pages.Account
{
    [LoginRequired]
    public class IndexModel : PageModel
    {
        public IndexModel(DatabaseContext database) => this.database = database;
        private readonly DatabaseContext database;

        public List<Models.Account> Accounts { get; private set; }
        public async Task OnGetAsync() => Accounts = await database.Accounts.Include(t => t.Sessions).ToListAsync();
    }
}
