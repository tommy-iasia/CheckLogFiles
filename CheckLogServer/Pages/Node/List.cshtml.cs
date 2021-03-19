using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckLogServer.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CheckLogServer.Pages.Node
{
    public class ListModel : PageModel
    {
        public ListModel(DatabaseContext database) => this.database = database;
        private readonly DatabaseContext database;

        public List<Models.Node> Nodes;
        public async Task OnGetAsync()
            => Nodes = await database.Nodes
            .OrderBy(t => t.Identitifer)
            .Include(t => t.LevelLog)
            .ToListAsync();
    }
}
