using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckLogServer.Filter;
using CheckLogServer.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CheckLogServer.Pages.Log
{
    [LoginRequired]
    public class IndexModel : PageModel
    {
        public IndexModel(LogRowSaver logRowSaver) => this.logRowSaver = logRowSaver;
        private readonly LogRowSaver logRowSaver;

        public LogRow[] AllRows { get; private set; }

        public IEnumerable<LogRow> PageRows { get; private set; }
        public (int index, int total) Pagination { get; private set; }

        public async Task OnGetAsync(int pageIndex = 0)
        {
            AllRows = await logRowSaver.LoadAsync();

            const int pageSize = 50;
            PageRows = AllRows.Skip(pageIndex * pageSize).Take(pageSize);

            var totalPage = (int)Math.Ceiling((double)AllRows.Length / pageSize);
            Pagination = (pageIndex, totalPage);
        }
    }
}
