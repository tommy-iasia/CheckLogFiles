using System.Linq;
using System.Threading.Tasks;
using CheckLogServer.Models;
using CheckLogUtility.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CheckLogServer.Pages.Node
{
    public class DetailsModel : PageModel
    {
        public DetailsModel(DatabaseContext database) => this.database = database;
        private readonly DatabaseContext database;

        public Models.Node Node { get; private set; }
        public LogLine[] Lines { get; private set; }
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Node = await database.Nodes
                .Where(t => t.Id == id)
                .Include(t => t.LevelLog)
                .FirstOrDefaultAsync();
            
            if (Node == null)
            {
                return NotFound();
            }

            if (Node.LevelLog != null
                && !Node.LevelLog.Deleted
                && System.IO.File.Exists(Node.LevelLog.FileName))
            {
                var text = await System.IO.File.ReadAllTextAsync(Node.LevelLog.FileName);
                Lines = Logger.Read(text);
            }

            return Page();
        }
    }
}
