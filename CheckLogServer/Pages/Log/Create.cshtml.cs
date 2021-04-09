using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CheckLogServer.Hubs;
using CheckLogServer.Models;
using CheckLogServer.Services;
using CheckLogUtility.Logging;
using CheckLogWorker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CheckLogServer.Pages.Log
{
    [IgnoreAntiforgeryToken]
    public class CreateModel : PageModel
    {
        public CreateModel(DatabaseContext database, IHubContext<NodeHub> nodeHub, TelegramService telegramService, EmailService emailService)
        {
            this.database = database;
            this.nodeHub = nodeHub;
            this.telegramService = telegramService;
            this.emailService = emailService;
        }
        private readonly DatabaseContext database;
        private readonly IHubContext<NodeHub> nodeHub;
        private readonly TelegramService telegramService;
        private readonly EmailService emailService;

        [FromBody]
        public LogSubmit Submit { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var name = Submit.Name.ToString();
            if (await database.LogRows.AnyAsync(t => t.Name == name))
            {
                return NotFound();
            }

            var directory = @"Data\Log";
            Directory.CreateDirectory(directory);

            var fileName = Path.Combine(directory, $"{name}.json");
            await System.IO.File.WriteAllTextAsync(fileName, Submit.Text);

            var lines = Logger.Read(Submit.Text);
            var newLevel = lines.GetLevel();

            var client = $"{HttpContext.Connection.RemoteIpAddress}:{HttpContext.Connection.RemotePort}";

            var row = new LogRow
            {
                Name = name,
                Time = Submit.Time,
                Level = newLevel,
                FileName = fileName,
                Deleted = false,
                Client = client
            };
            database.LogRows.Add(row);

            var node = await database.Nodes
                .Where(t => t.Identifier == Submit.Name.Identifier)
                .Include(t => t.LevelLog)
                .FirstOrDefaultAsync();

            if (node != null)
            {
                if (node.Disabled)
                {
                    return NotFound();
                }
            }
            else
            {
                node = new Models.Node
                {
                    Identifier = Submit.Name.Identifier,
                    Disabled = false
                };
                database.Nodes.Add(node);
            }

            node.LogTime = DateTime.Now;

            var oldLevel = node.LevelLog?.Level ?? LogLevel.Unknown;
            if (newLevel > LogLevel.Info
                && newLevel >= oldLevel)
            {
                node.LevelLog = row;
                node.LevelTime = node.LogTime;
            }

            await database.SaveChangesAsync();

            _ = nodeHub.NodeAsync(node);

            if (newLevel > LogLevel.Info
                && newLevel > oldLevel)
            {
                _ = telegramService.SendMessageAsync($"*{newLevel}* `{node.Identifier}`");

                _ = emailService.SendMessageAsync(
                    $"{node.Identifier} has reached {newLevel} level",
                    t => $"<a href=\"{t.ServerAddress}/Node/Details?id={node.Id}\">Click Here</a>");
            }

            return Content($"{row.Name} created");
        }
    }
}
