using CheckLogUtility.Linq;
using CheckLogUtility.Logging;
using CheckLogUtility.Randomize;
using CheckLogUtility.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckLogScheduler
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var logger = new Logger();

            if (!args.Any())
            {
                await logger.ErrorAsync("No configuration is provided");
                return;
            }

            await logger.InfoAsync("Prepare");

            var configures = await Configure.LoadAsync(args);

            var slots = await configures
                .SelectAsync(async t => await GetSlotAsync(t, logger))
                .ToArrayAsync();

            await logger.InfoAsync("Start");

            var singleton = await Singleton.GainAsync();
            await logger.InfoAsync($"Gain singleton with {singleton.Guid}");

            await logger.SetFileAsync($"{nameof(CheckLogScheduler)}\\{DateTime.Now:yyyyMMddHHmmss}.{RandomUtility.Next(9999)}.log");

            await RunSlotsAsync(slots, singleton, logger);
        }

        private static async Task<Slot> GetSlotAsync(Configure configure, Logger logger)
        {
            await logger.InfoAsync($"Configure {configure.Time} with {string.Join(", ", configure.Arguments)}");

            var timeExpression = TimeExpression.Parse(configure.Time);

            return new Slot
            {
                TimeExpression = timeExpression,
                NextTime = timeExpression.Next(DateTime.Now),
                Arguments = configure.Arguments
            };
        }

        private static async Task RunSlotsAsync(IEnumerable<Slot> slots, Singleton singleton, Logger logger)
        {
            while (true)
            {
                var (valid, guid) = await singleton.ValidateAsync();
                if (!valid)
                {
                    await logger.InfoAsync($"Overriding by new scheduler {guid}");
                    break;
                }

                var slot = slots.OrderBy(t => t.NextTime).First();

                await logger.InfoAsync($"Next worker will be run at {slot.NextTime:yy/MM/dd HH:mm:ss} with {string.Join(", ", slot.Arguments)}");

                var totalSpan = slot.NextTime - DateTime.Now;
                await logger.InfoAsync($"Wait for {totalSpan.TotalSeconds:#,##0}s");

                await slot.WaitAsync();

                Console.WriteLine();

                await slot.RunAsync(logger);

                Console.WriteLine();

                slot.Next();

                await logger.InfoAsync($"This worker will be run again at {slot.NextTime:yy/MM/dd HH:mm:ss} next time");
            }
        }
    }
}
