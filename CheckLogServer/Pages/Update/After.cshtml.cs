using System.Linq;
using System.Threading.Tasks;
using CheckLogServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CheckLogServer.Pages.Update
{
    public class AfterModel : PageModel
    {
        public AfterModel(DatabaseContext database) => this.database = database;
        private readonly DatabaseContext database;

        public async Task<JsonResult> OnGetAsync(string version)
        {
            var onUpdates = await database.Updates
                .Where(t => t.Status == UpdateStatus.On)
                .ToListAsync();

            var outputUpdates = onUpdates
                .Where(t => t.Version.CompareTo(version) > 0)
                .OrderBy(t => t.Version)
                .ToArray();

            return new JsonResult(outputUpdates);
        }
    }
}
