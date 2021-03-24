using CheckLogUtility.Logging;
using CheckLogUtility.Randomize;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CheckLogUpdater
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var logger = new Logger();
            
            await logger.InfoAsync("Start");

            try
            {
                await logger.SetFileAsync($"{nameof(CheckLogUpdater)}.{DateTime.Now:yyMMddHHmmss}.{RandomUtility.Next(9999)}.log");

                await RunAsync(args, logger);
            }
            catch (Exception e)
            {
                await logger.ErrorAsync(e);
            }

            await logger.InfoAsync("End");
        }

        private static async Task RunAsync(string[] args, Logger logger)
        {
            var action = args.First();
            switch (action)
            {
                case nameof(Prepare):
                    await Prepare.RunAsync(logger);
                    break;

                case nameof(Continue):
                    var arguments = args.Skip(1);

                    await Continue.RunAsync(arguments, logger);
                    break;

                default:
                    throw new ArgumentException(null, nameof(args));
            }
        }
    }
}
