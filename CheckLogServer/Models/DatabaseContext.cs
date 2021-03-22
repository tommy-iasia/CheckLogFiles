using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace CheckLogServer.Models
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountSession> AccountSessions { get; set; }

        public DbSet<LogRow> LogRows { get; set; }

        public DbSet<Node> Nodes { get; set; }

        public DbSet<Update> Updates { get; set; }

        public DbSet<Telegram> Telegrams { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlite(@"Data Source=Database.db");
        }

        public static void Initialize(IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            context.Database.EnsureCreated();

            InitializeAsync(context).Wait();
        }
        private static async Task InitializeAsync(DatabaseContext context)
        {
            if (await context.Accounts.AnyAsync())
            {
                return;
            }

            context.Accounts.Add(new Account
            {
                Username = "Tommy",
                Password = "123456",
            });

            context.Accounts.Add(new Account
            {
                Username = "Pikachu",
                Password = "123456",
            });

            await context.SaveChangesAsync();
        }
    }
}
