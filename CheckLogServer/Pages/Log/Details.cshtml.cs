using System.Threading.Tasks;
using CheckLogServer.Models;
using CheckLogWorker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CheckLogServer.Pages.Log
{
    public class DetailsModel : PageModel
    {
        public DetailsModel(DatabaseContext database) => this.database = database;
        private readonly DatabaseContext database;

        public LogRow Row { get; private set; }
        public LogLine[] Lines { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Row = await database.LogRows.FindAsync(id);
            if (Row == null)
            {
                return NotFound();
            }

            if (!Row.Deleted && System.IO.File.Exists(Row.FileName))
            {
                var text = await System.IO.File.ReadAllTextAsync(Row.FileName);
                Lines = Logger.Read(text);
            }

            return Page();
        }
    }
}
