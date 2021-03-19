using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckLogServer.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CheckLogServer.Pages.Update
{
    public class IndexModel : PageModel
    {
        public IndexModel(DatabaseContext database) => this.database = database;
        private readonly DatabaseContext database;

        public List<Models.Update> Updates { get; private set; }
        public (int index, int total) Pagination { get; private set; }
        public async Task OnGetAsync(int pageIndex = 0)
        {
            var count = await database.LogRows.CountAsync();

            const int pageSize = 10;
            Updates = await database.Updates
                .Skip(pageIndex * pageSize).Take(pageSize)
                .ToListAsync();

            var totalPage = (int)Math.Ceiling((double)count / pageSize);
            Pagination = (pageIndex, totalPage);
        }
    }
}
