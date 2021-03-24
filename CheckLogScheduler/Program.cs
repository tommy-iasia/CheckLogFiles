using CheckLogUtility.Linq;
using CheckLogUtility.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CheckLogScheduler
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var logger = new Logger();

            await logger.InfoAsync("Prepare");

            var configures = await Configure.LoadAsync(args);

            if (configures.Length <= 0)
            {
                await logger.ErrorAsync("No configure is provided.");
                return;
            }

            var slots = await configures.SelectAsync(async t =>
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

            await logger.InfoAsync("Start");

            var singleton = await Singleton.GainAsync();
            await logger.InfoAsync($"Gain singleton with {singleton.Guid}");

            var program = new Program(slots);

            while (true)
            {
                var (valid, guid) = await singleton.ValidateAsync();
                if (!valid)
                {
                    await logger.InfoAsync($"Overriding by new scheduler {guid}");
                }

                await program.NextAsync(logger);
            }
        }

        public Program(Slot[] slots) => this.slots = slots;
        private readonly Slot[] slots;

        private async Task NextAsync(Logger logger)
        {
            var slot = slots.OrderBy(t => t.NextTime).First();
            await slot.WaitAsync();

            await slot.RunAsync(logger);

            slot.Next();
            await logger.InfoAsync($"Worker will be run at {slot.NextTime:yy/MM/dd HH:mm:ss} next time");
        }
    }
}
