using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CheckLogServer.Models;
using CheckLogWorker;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CheckLogServer.Pages.Log
{
    [IgnoreAntiforgeryToken]
    public class CreateModel : PageModel
    {
        public CreateModel(LogRowSaver logRowSaver) => this.logRowSaver = logRowSaver;
        private readonly LogRowSaver logRowSaver;

        [FromBody]
        public LogSubmit Submit { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var oldRows = await logRowSaver.LoadAsync();
            if (oldRows.Any(t => t.Name == Submit.Name))
            {
                return NotFound();
            }

            var directory = @"Data\Log";
            Directory.CreateDirectory(directory);

            var fileName = Path.Combine(directory, $"{Submit.Name}.json");
            await System.IO.File.WriteAllTextAsync(fileName, Submit.Text);

            var lines = Logger.Read(Submit.Text);
            var level = lines.GetLevel();

            var row = new LogRow
            {
                Name = Submit.Name,
                Time = Submit.Time,
                Level = level,
                FileName = fileName,
                Deleted = false
            };

            var newRows = new[] { row }.Concat(oldRows).ToArray();
            await logRowSaver.SaveAsync(newRows);

            return Content($"{row.Name} created");
        }
    }
}
