using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckLogServer.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CheckLogServer.Pages.Log
{
    public class IndexModel : PageModel
    {
        public IndexModel(DatabaseContext database) => this.database = database;
        private readonly DatabaseContext database;

        public List<LogRow> Rows { get; private set; }
        public (int index, int total) Pagination { get; private set; }

        public async Task OnGetAsync(int pageIndex = 0)
        {
            var count = await database.LogRows.CountAsync();

            const int pageSize = 100;
            Rows = await database.LogRows.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();

            var totalPage = (int)Math.Ceiling((double)count / pageSize);
            Pagination = (pageIndex, totalPage);
        }
    }
}
