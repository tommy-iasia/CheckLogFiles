using System;
using System.Linq;
using System.Threading.Tasks;

namespace CheckLogUpdater
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Start");

            try
            {
                await RunAsync(args);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }

            Console.WriteLine("End");
        }

        private static async Task RunAsync(string[] args)
        {
            var action = args.First();
            switch (action)
            {
                case nameof(Prepare):
                    await Prepare.RunAsync();
                    break;

                case nameof(Continue):
                    var arguments = args.Skip(1);

                    await Continue.RunAsync(arguments);
                    break;

                default:
                    throw new ArgumentException(null, nameof(args));
            }
        }
    }
}
