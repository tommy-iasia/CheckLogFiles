using System.Threading.Tasks;
using CheckLogServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CheckLogServer.Pages.Account
{
    public class IndexModel : PageModel
    {
        public IndexModel(AccountsSaver saver) => this.saver = saver;
        private readonly AccountsSaver saver;

        public Models.Account[] Accounts { get; private set; }
        public async Task OnGetAsync() => Accounts = await saver.LoadAsync();
    }
}
