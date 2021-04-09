using CheckLogServer.Models;
using CheckLogUtility.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CheckLogServer.Services
{
    public class TelegramService
    {
        public TelegramService(DatabaseContext database) => this.database = database;
        private readonly DatabaseContext database;

        public async Task<bool> SendMessageAsync(string text)
        {
            var telegrams = await database.Telegrams.ToListAsync();
            if (telegrams.Count <= 0)
            {
                return false;
            }

            using var client = new HttpClient();
            var textEncoded = WebUtility.UrlEncode(text);

            foreach (var t in telegrams)
            {
                try
                {
                    var result = await client.GetStringAsync(@$"https://api.telegram.org/bot{t.BotToken}/sendMessage?chat_id={t.ChannelId}&text={textEncoded}&parse_mode=MarkdownV2");
                    if (result?.Contains("\"ok\":true") ?? false)
                    {
                        return true;
                    }
                }
                catch (Exception) { }
            }

            return false;
        }
    }
}
