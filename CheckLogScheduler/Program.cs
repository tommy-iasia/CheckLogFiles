using CheckLogUtility.Linq;
using CheckLogUtility.Logging;
using CheckLogUtility.Randomize;
using CheckLogUtility.Timing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
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

            var slots = await GetSlotsAsync(args, logger);

            await logger.InfoAsync("Start");

            var singleton = await Singleton.GainAsync();
            await logger.InfoAsync($"Gain singleton with {singleton.Guid}");

            await logger.SetFileAsync($"{nameof(CheckLogScheduler)}\\{DateTime.Now:yyyyMMddHHmmss}.{RandomUtility.Next(9999)}.log");

            await RunSlotsAsync(slots, singleton, logger);
        }

        private static async Task<Slot[]> GetSlotsAsync(string[] arguments, Logger logger)
        {
            var configures = await Configure.LoadAsync(arguments);

            var runner = GetRunner();

            return await configures.SelectAsync(async t =>
            {
                await logger.InfoAsync($"Configure {t.Time} with {string.Join(", ", t.Arguments)}");

                var timeExpression = TimeExpression.Parse(t.Time);

                return new Slot
                {
                    TimeExpression = timeExpression,
                    NextTime = timeExpression.Next(DateTime.Now),
                    Runner = runner,
                    Arguments = t.Arguments
                };
            }).ToArrayAsync();
        }

        private static Func<string[], CancellationToken, Task> GetRunner()
        {
            const string workerName = "CheckLogWorker";
            var fileNames = new[]
            {
                $"{workerName}.exe",
                $"{workerName}.dll",
                $"ref\\{workerName}.dll",
                $"{workerName}\\{workerName}.exe",
                $"{workerName}\\{workerName}.dll"
            };

            var assemblies = fileNames.Where(File.Exists).Select(t =>
            {
                try
                {
                    var assembly = Assembly.LoadFrom(t);
                    return assembly;
                }
                catch (Exception e)
                {
                    return (object)e;
                }
            }).ToArray();

            var methodName = "RunAsync";
            var method = assemblies.OfType<Assembly>()
                .Select(t => t.GetType($"{workerName}.{nameof(Program)}"))
                .Where(t => t != null)
                .Select(t => t.GetMethod(methodName, new[] { typeof(string[]), typeof(CancellationToken) }))
                .Where(t => t?.ReturnType == typeof(Task))
                .FirstOrDefault();

            if (method == null)
            {
                throw new InvalidOperationException($"Cannot find {fileNames.First()} or valid {methodName}.");
            }

            return async (string[] arguments, CancellationToken cancellationToken)
                => await (Task)method.Invoke(null, new object[] { arguments, cancellationToken });
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
