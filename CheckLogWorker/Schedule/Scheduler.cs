using CheckLogUtility.Linq;
using CheckLogUtility.Logging;
using CheckLogUtility.Randomize;
using CheckLogUtility.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckLogWorker.Schedule
{
    public class Scheduler
    {
        public static async Task RunAsync(string[] arguments)
        {
            var logger = new Logger();

            if (!arguments.Any())
            {
                await logger.ErrorAsync("No configuration is provided");
                return;
            }

            await logger.InfoAsync("Prepare");

            var slots = await GetSlotsAsync(arguments, logger);

            await logger.InfoAsync("Start");

            var singleton = await Singleton.GainAsync();
            await logger.InfoAsync($"Gain singleton with {singleton.Guid}");

            await logger.SetFileAsync($"{nameof(CheckLogWorker)}\\{nameof(Schedule)}\\{DateTime.Now:yyyyMMddHHmmss}.{RandomUtility.Next(9999)}.log");

            await RunSlotsAsync(slots, singleton, logger);
        }

        private static async Task<Slot[]> GetSlotsAsync(string[] arguments, Logger logger)
        {
            var configures = await Configure.LoadAsync(arguments);

            return await configures.SelectAsync(async t =>
            {
                await logger.InfoAsync($"Configure {t.Time} with {string.Join(", ", t.Arguments)}");

                var timeExpression = TimeExpression.Parse(t.Time);

                return new Slot
                {
                    TimeExpression = timeExpression,
                    NextTime = timeExpression.Next(DateTime.Now),
                    Arguments = t.Arguments
                };
            }).ToArrayAsync();
        }

        private static async Task RunSlotsAsync(IEnumerable<Slot> slots, Singleton singleton, Logger logger)
        {
            while (true)
            {
                var (firstValid, firstGuid) = await singleton.ValidateAsync();
                if (!firstValid)
                {
                    await logger.InfoAsync($"Overriding by new scheduler {firstGuid}");
                    break;
                }

                var slot = slots.OrderBy(t => t.NextTime).First();

                await logger.InfoAsync($"Next worker will be run at {slot.NextTime:yy/MM/dd HH:mm:ss} with {string.Join(", ", slot.Arguments)}");

                var totalSpan = slot.NextTime - DateTime.Now;
                await logger.InfoAsync($"Wait for {totalSpan.TotalSeconds:#,##0}s");

                while (true)
                {
                    var (secondValid, secondGuid) = await singleton.ValidateAsync();
                    if (!secondValid)
                    {
                        await logger.InfoAsync($"Overriding by new scheduler {secondGuid}");
                        break;
                    }

                    var waitTime = slot.NextTime - DateTime.Now;
                    if (waitTime <= TimeSpan.Zero)
                    {
                        break;
                    }

                    await Task.Delay(
                        waitTime.TotalSeconds <= 5 ? waitTime
                        : TimeSpan.FromSeconds(5));
                }

                Console.WriteLine();

                await slot.RunAsync(logger);

                Console.WriteLine();

                slot.Next();

                await logger.InfoAsync($"This worker will be run again at {slot.NextTime:yy/MM/dd HH:mm:ss} next time");
            }
        }
    }
}
