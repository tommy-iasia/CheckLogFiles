using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CheckLogServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CheckLogServer.Pages.Update
{
    public class DownloadModel : PageModel
    {
        public DownloadModel(DatabaseContext database) => this.database = database;
        private readonly DatabaseContext database;

        public async Task<IActionResult> OnGetAsync(int id, string name = null)
        {
            var update = await database.Updates.FindAsync(id);
            if (update == null)
            {
                return NotFound();
            }
            if (update.Status != UpdateStatus.On)
            {
                return NotFound();
            }

            var directoryPath = Path.Combine(@"Data\Update", id.ToString());

            if (name != null)
            {
                var filePath = Path.Combine(directoryPath, name);
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound();
                }

                var stream = System.IO.File.OpenRead(filePath);
                return File(stream, "application/octet-stream");
            }
            else
            {
                var fullPaths = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
                var relativePaths = fullPaths.Select(t => t[(directoryPath.Length + 1)..]);

                return new JsonResult(relativePaths);
            }
        }
    }
}
