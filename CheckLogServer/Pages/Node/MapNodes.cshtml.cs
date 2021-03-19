using System.Threading.Tasks;
using CheckLogServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CheckLogServer.Pages.Node
{
    public class MapNodesModel : PageModel
    {
        public MapNodesModel(DatabaseContext database) => this.database = database;
        private readonly DatabaseContext database;

        public async Task<JsonResult> OnGetAsync()
        {
            var nodes = await database.Nodes.Include(t => t.LevelLog).ToListAsync();
            return new JsonResult(nodes);
        }
    }
}
