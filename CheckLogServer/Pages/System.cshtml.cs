using CheckLogServer.Filter;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CheckLogServer.Pages
{
    [LoginRequired]
    public class SystemModel : PageModel
    {
        public void OnGet() { }
    }
}
