using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CheckLogRunner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configures = await Configure.LoadAsync(args);

            if (configures.Length <= 0)
            {
                Console.WriteLine("No configure is provided.");
            }

            var slots = configures.Select(t =>
            {
                Console.WriteLine($"Configure {t.Time} with {string.Join(", ", t.Arguments)}");

                var timeExpression = TimeExpression.Parse(t.Time);

                return new Slot
                {
                    Configure = t,
                    TimeExpression = timeExpression,
                    NextTime = timeExpression.Next(DateTime.Now)
                };
            }).ToArray();

            Console.WriteLine("Start");

            var program = new Program(slots);

            while (true)
            {
                await program.NextAsync();
            }
        }

        public Program(Slot[] slots) => this.slots = slots;
        private readonly Slot[] slots;

        private async Task NextAsync()
        {
            var slot = await WaitAsync();

            await RunAsync(slot);
        }
        private async Task<Slot> WaitAsync()
        {
            while (true)
            {
                var nextSlot = slots.OrderBy(t => t.NextTime).First();

                var now = DateTime.Now;
                if (nextSlot.NextTime >= now)
                {
                    return nextSlot;
                }

                var timeSpan = nextSlot.NextTime - now;
                if (timeSpan.TotalSeconds > 5)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
                else
                {
                    await Task.Delay(timeSpan);
                }
            }
        }
        private async Task RunAsync(Slot slot)
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime:yy/MM/dd HH:mm:ss}] Start worker with {string.Join(", ", slot.Configure.Arguments)}");

            await CheckLogWorker.Program.Main(slot.Configure.Arguments);

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime:yy/MM/dd HH:mm:ss}] Complete worker within {(endTime - startTime).TotalSeconds:#,##0}s");

            slot.NextTime = slot.TimeExpression.Next(endTime);
            Console.WriteLine($"[{DateTime.Now:yy/MM/dd HH:mm:ss}] Worker will be run at {slot.NextTime:yy/MM/dd HH:mm:ss} next time");
        }
    }
}
