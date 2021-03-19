using System;
using System.Linq;
using System.Threading.Tasks;
using CheckLogServer.Hubs;
using CheckLogServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CheckLogServer.Pages.Node
{
    public class ResolveModel : PageModel
    {
        public ResolveModel(DatabaseContext database, IHubContext<NodeHub> nodeHub)
        {
            this.database = database;
            this.nodeHub = nodeHub;
        }
        private readonly DatabaseContext database;
        private readonly IHubContext<NodeHub> nodeHub;

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var node = await database.Nodes
                .Where(t => t.Id == id)
                .Include(t => t.LevelLog)
                .FirstOrDefaultAsync();

            if (node == null)
            {
                return NotFound();
            }

            node.LevelLog = null;
            node.LevelTime = DateTime.Now;

            await database.SaveChangesAsync();

            var _ = nodeHub.NodeAsync(node);

            return RedirectToPage("./Details", new { id });
        }
    }
}
