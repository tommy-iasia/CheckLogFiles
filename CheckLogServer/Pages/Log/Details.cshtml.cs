using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckLogServer.Models;
using CheckLogWorker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CheckLogServer.Pages.Log
{
    public class DetailsModel : PageModel
    {
        public DetailsModel(LogRowSaver logRowSaver) => this.logRowSaver = logRowSaver;
        private readonly LogRowSaver logRowSaver;

        public LogRow Row { get; private set; }
        public LogLine[] Lines { get; private set; }

        public async Task<IActionResult> OnGetAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest();
            }

            var rows = await logRowSaver.LoadAsync();
            Row = rows.FirstOrDefault(t => t.Name.ToString() == name);

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
